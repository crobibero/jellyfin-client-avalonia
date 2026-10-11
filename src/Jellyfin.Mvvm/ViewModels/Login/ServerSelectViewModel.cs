using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// Server select view model.
/// </summary>
public sealed partial class ServerSelectViewModel : BaseViewModel, IDisposable
{
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;
    private readonly IServerDiscoveryService _serverDiscoveryService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IDeviceInfo _deviceInfo;
    private readonly CancellationTokenSource _discoveryCancellation = new();

    [ObservableProperty]
    private bool _isDiscovering;

    [ObservableProperty]
    private bool _hasServers;

    [ObservableProperty]
    private bool _hasDiscoveredServers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="serverDiscoveryService">Instance of the <see cref="IServerDiscoveryService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="deviceInfo">Instance of the <see cref="IDeviceInfo"/> interface.</param>
    public ServerSelectViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        IStateService stateService,
        IServerDiscoveryService serverDiscoveryService,
        IAuthenticationService authenticationService,
        IDeviceInfo deviceInfo)
        : base(navigationService, applicationService)
    {
        _deviceInfo = deviceInfo;
        _stateStorageService = stateStorageService;
        _stateService = stateService;
        _serverDiscoveryService = serverDiscoveryService;
        _authenticationService = authenticationService;
    }

    /// <inheritdoc />
    public override bool ShowToolbar => false;

    /// <summary>
    /// Gets a value indicating whether servers can be discovered on the local network.
    /// </summary>
    public bool CanDiscover => _deviceInfo.SupportsServerDiscovery;

    /// <summary>
    /// Gets the list of saved servers.
    /// </summary>
    public ObservableRangeCollection<ServerStateModel> Servers { get; } = new();

    /// <summary>
    /// Gets the list of servers found on the local network.
    /// </summary>
    public ObservableCollection<ServerDiscoveryResponse> DiscoveredServers { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _discoveryCancellation.Cancel();
        _discoveryCancellation.Dispose();
    }

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        await LoadServersAsync().ConfigureAwait(true);
        DiscoverAsync().SafeFireAndForget();
    }

    /// <inheritdoc />
    protected override async ValueTask OnReturnedAsync() => await LoadServersAsync().ConfigureAwait(true);

    private async Task LoadServersAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(true);
        Servers.ReplaceRange(state.Servers.OrderByDescending(s => s.LastUsed ?? DateTimeOffset.MinValue));
        HasServers = Servers.Count > 0;
    }

    [RelayCommand]
    private async Task DiscoverAsync()
    {
        if (IsDiscovering)
        {
            return;
        }

        IsDiscovering = true;
        try
        {
            await foreach (var server in _serverDiscoveryService.DiscoverAsync(TimeSpan.FromSeconds(4), _discoveryCancellation.Token).ConfigureAwait(true))
            {
                var isSaved = Servers.Any(s => string.Equals(s.Id.ToString("N"), server.Id?.Replace("-", string.Empty, StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase));
                if (!isSaved && !DiscoveredServers.Any(s => s.Id == server.Id))
                {
                    DiscoveredServers.Add(server);
                    HasDiscoveredServers = true;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Navigated away.
        }
        finally
        {
            IsDiscovering = false;
        }
    }

    [RelayCommand]
    private async Task AddServerAsync()
    {
        await NavigationService.NavigateAsync<AddServerViewModel>().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task SelectServerAsync(ServerStateModel? server)
    {
        if (server is null)
        {
            return;
        }

        server.LastUsed = DateTimeOffset.UtcNow;
        _stateService.SetServerState(server);
        await _stateStorageService.AddServerAsync(server).ConfigureAwait(true);
        await NavigationService.NavigateAsync<SelectUserViewModel>().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task ConnectDiscoveredAsync(ServerDiscoveryResponse? server)
    {
        if (server?.Address is null)
        {
            return;
        }

        Loading = true;
        ErrorMessage = null;
        try
        {
            var info = await _authenticationService.ResolveServerAsync(server.Address).ConfigureAwait(true);
            if (info is null)
            {
                ErrorMessage = Strings.Strings.Login_UnableToConnect;
                return;
            }

            await SelectServerAsync(new ServerStateModel { Id = info.Id, Name = info.Name, Url = info.Url, Version = info.Version }).ConfigureAwait(true);
        }
        finally
        {
            Loading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteServerAsync(ServerStateModel? server)
    {
        if (server is not null)
        {
            await _stateStorageService.RemoveServerAsync(server.Id).ConfigureAwait(true);
            await LoadServersAsync().ConfigureAwait(true);
        }
    }
}
