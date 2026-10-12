using Jellyfin.Sdk.WebSockets;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc cref="IServerEventService" />
public sealed class ServerEventService : IServerEventService, IDisposable
{
    private readonly JellyfinWebSocketClient _webSocketClient;
    private readonly JellyfinSdkSettings _sdkSettings;
    private readonly IStateService _stateService;
    private readonly IApplicationService _applicationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ServerEventService> _logger;
    private readonly Lock _lock = new();

    private List<IDisposable>? _subscriptions;
    private (string? ServerUrl, string? AccessToken) _target;
    private bool _started;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventService"/> class.
    /// </summary>
    /// <param name="webSocketClient">The WebSocket client.</param>
    /// <param name="sdkSettings">The sdk settings.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="logger">The logger.</param>
    public ServerEventService(
        JellyfinWebSocketClient webSocketClient,
        JellyfinSdkSettings sdkSettings,
        IStateService stateService,
        IApplicationService applicationService,
        INotificationService notificationService,
        ILogger<ServerEventService> logger)
    {
        _webSocketClient = webSocketClient;
        _sdkSettings = sdkSettings;
        _stateService = stateService;
        _applicationService = applicationService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<UserDataChangeInfo>? UserDataChanged;

    /// <inheritdoc />
    public event EventHandler<LibraryUpdateInfo>? LibraryChanged;

    /// <inheritdoc />
    public event EventHandler<PlayRequest>? PlayRequested;

    /// <inheritdoc />
    public event EventHandler<PlaystateRequest>? PlaystateRequested;

    /// <inheritdoc />
    public event EventHandler<GeneralCommand>? GeneralCommandReceived;

    /// <inheritdoc />
    public void Start()
    {
        lock (_lock)
        {
            if (_started || _disposed)
            {
                return;
            }

            _started = true;
        }

        _webSocketClient.StateChanged += OnStateChanged;
        _webSocketClient.ErrorOccurred += OnErrorOccurred;
        _stateService.UserChanged += OnUserChanged;
        UpdateSubscriptions();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Unsubscribe();
        }

        _webSocketClient.StateChanged -= OnStateChanged;
        _webSocketClient.ErrorOccurred -= OnErrorOccurred;
        _stateService.UserChanged -= OnUserChanged;
    }

    private void OnUserChanged(object? sender, EventArgs e) => UpdateSubscriptions();

    /// <summary>
    /// Subscribe while a user is signed in, so the connection only ever uses a complete server url and token pair.
    /// </summary>
    private void UpdateSubscriptions()
    {
        var target = (_sdkSettings.ServerUrl, _sdkSettings.AccessToken);
        var signedIn = _stateService.CurrentUser is not null
            && !string.IsNullOrEmpty(target.ServerUrl)
            && !string.IsNullOrEmpty(target.AccessToken);

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            if (!signedIn)
            {
                Unsubscribe();
                return;
            }

            if (_subscriptions is null)
            {
                // The first subscription opens the connection.
                _target = target;
                _subscriptions = Subscribe();
            }
            else if (target != _target)
            {
                _target = target;
                _webSocketClient.Reconnect();
            }
        }
    }

    private List<IDisposable> Subscribe() =>
    [
        _webSocketClient.Subscribe<UserDataChangedMessage>(m => Raise(m.Data, () => UserDataChanged)),
        _webSocketClient.Subscribe<LibraryChangedMessage>(m => Raise(m.Data, () => LibraryChanged)),
        _webSocketClient.Subscribe<PlayMessage>(m => Raise(m.Data, () => PlayRequested)),
        _webSocketClient.Subscribe<PlaystateMessage>(m => Raise(m.Data, () => PlaystateRequested)),
        _webSocketClient.Subscribe<GeneralCommandMessage>(m => Raise(m.Data, () => GeneralCommandReceived)),
        _webSocketClient.Subscribe<UserUpdatedMessage>(OnUserUpdated),
        _webSocketClient.Subscribe<ServerRestartingMessage>(_ => _notificationService.Show(Strings.Strings.Server_Restarting)),
        _webSocketClient.Subscribe<ServerShuttingDownMessage>(_ => _notificationService.Show(Strings.Strings.Server_ShuttingDown)),
    ];

    /// <summary>
    /// Dispose the subscriptions, which closes the connection. Must be called while holding <see cref="_lock"/>.
    /// </summary>
    private void Unsubscribe()
    {
        if (_subscriptions is null)
        {
            return;
        }

        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        _subscriptions = null;
        _target = default;
    }

    /// <summary>
    /// Raise an event on the UI thread. The handler is read there, so handlers removed in the meantime are not called.
    /// </summary>
    private void Raise<T>(T? data, Func<EventHandler<T>?> getHandler)
        where T : class
    {
        if (data is null)
        {
            return;
        }

        _applicationService.Post(() =>
        {
            try
            {
                getHandler()?.Invoke(this, data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to handle {Message}", typeof(T).Name);
            }
        });
    }

    private void OnUserUpdated(UserUpdatedMessage message)
    {
        if (message.Data is not { Id: { } id } user)
        {
            return;
        }

        _applicationService.Post(() =>
        {
            if (_stateService.CurrentUser?.Id == id)
            {
                _stateService.SetUser(user);
            }
        });
    }

    private void OnStateChanged(object? sender, WebSocketStateChangedEventArgs e)
    {
        if (e.Error is null)
        {
            _logger.LogDebug("WebSocket {State}", e.State);
        }
        else
        {
            _logger.LogInformation(e.Error, "WebSocket {State}", e.State);
        }
    }

    private void OnErrorOccurred(object? sender, TypedEventArgs<Exception> e)
        => _logger.LogWarning(e.Content, "WebSocket message could not be handled");
}
