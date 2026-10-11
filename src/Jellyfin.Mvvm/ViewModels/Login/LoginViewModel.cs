using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// Login view model.
/// </summary>
public sealed partial class LoginViewModel : BaseViewModel, IDisposable
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IStateService _stateService;
    private readonly IStateStorageService _stateStorageService;
    private readonly PlaybackManager _playbackManager;
    private CancellationTokenSource? _quickConnectCancellation;

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    private Guid _serverId;

    [ObservableProperty]
    private string _serverName = string.Empty;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private bool _rememberMe = true;

    [ObservableProperty]
    private bool _quickConnectAvailable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsQuickConnectActive))]
    private string? _quickConnectCode;

    [ObservableProperty]
    private bool _isSigningIn;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="playbackManager">The playback manager.</param>
    public LoginViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateService stateService,
        IStateStorageService stateStorageService,
        PlaybackManager playbackManager)
        : base(navigationService, applicationService)
    {
        _authenticationService = authenticationService;
        _stateService = stateService;
        _stateStorageService = stateStorageService;
        _playbackManager = playbackManager;
    }

    /// <inheritdoc />
    public override bool ShowToolbar => false;

    /// <summary>
    /// Gets a value indicating whether a Quick Connect request is pending.
    /// </summary>
    public bool IsQuickConnectActive => !string.IsNullOrEmpty(QuickConnectCode);

    /// <inheritdoc />
    public override void OnNavigatedFrom()
    {
        _quickConnectCancellation?.Cancel();
        QuickConnectCode = null;
        base.OnNavigatedFrom();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _quickConnectCancellation?.Cancel();
        _quickConnectCancellation?.Dispose();
    }

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        var server = _stateService.GetServerState();
        if (server is null)
        {
            await NavigationService.NavigateRootAsync<ServerSelectViewModel>().ConfigureAwait(true);
            return;
        }

        ServerId = server.Id;
        ServerUrl = server.Url;
        ServerName = server.Name;
        Title = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Login_SignInTo, server.Name);

        if (parameter is LoginRequest request)
        {
            Username = request.Username;
        }

        QuickConnectAvailable = await _authenticationService.IsQuickConnectEnabledAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = Strings.Strings.Login_UsernameRequired;
            return;
        }

        IsSigningIn = true;
        ErrorMessage = null;
        try
        {
            var (status, errorMessage) = await _authenticationService.AuthenticateAsync(ServerUrl, Username.Trim(), Password)
                .ConfigureAwait(true);
            if (status)
            {
                await CompleteLoginAsync().ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = errorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Login_UnknownError, ex.Message);
        }
        finally
        {
            IsSigningIn = false;
        }
    }

    [RelayCommand]
    private async Task LoginWithQuickConnectAsync()
    {
        _quickConnectCancellation?.Cancel();
        _quickConnectCancellation?.Dispose();
        _quickConnectCancellation = new CancellationTokenSource();
        var cancellationToken = _quickConnectCancellation.Token;

        ErrorMessage = null;
        try
        {
            var code = await _authenticationService.InitializeQuickConnectAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrEmpty(code))
            {
                ErrorMessage = Strings.Strings.Login_QuickConnectUnavailable;
                return;
            }

            QuickConnectCode = code;

            bool? authenticated;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(true);
                authenticated = await _authenticationService.TestQuickConnectAsync(cancellationToken).ConfigureAwait(true);
            }
            while (authenticated == false);

            QuickConnectCode = null;
            if (authenticated != true)
            {
                ErrorMessage = Strings.Strings.Login_QuickConnectUnavailable;
                return;
            }

            var (status, errorMessage) = await _authenticationService.AuthenticateWithQuickConnectAsync(cancellationToken).ConfigureAwait(true);
            if (status)
            {
                await CompleteLoginAsync().ConfigureAwait(true);
            }
            else
            {
                ErrorMessage = errorMessage;
            }
        }
        catch (OperationCanceledException)
        {
            QuickConnectCode = null;
        }
        catch (Exception ex)
        {
            QuickConnectCode = null;
            ErrorMessage = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Login_UnknownError, ex.Message);
        }
    }

    [RelayCommand]
    private void CancelQuickConnect()
    {
        _quickConnectCancellation?.Cancel();
        QuickConnectCode = null;
    }

    private async Task CompleteLoginAsync()
    {
        var user = _stateService.GetCurrentUser();
        var userState = new UserStateModel
        {
            Id = user.Id!.Value,
            ServerId = ServerId,
            Name = user.Name ?? Username ?? string.Empty,
            Token = RememberMe ? _stateService.GetToken() : null,
            ImageTag = user.PrimaryImageTag,
            LastUsed = DateTimeOffset.UtcNow,
        };

        await _stateStorageService.AddUserAsync(userState).ConfigureAwait(true);
        _stateService.SetUserState(userState);
        _playbackManager.ReportCapabilitiesAsync().SafeFireAndForget();
        await NavigationService.NavigateRootAsync<HomeViewModel>().ConfigureAwait(true);
    }
}
