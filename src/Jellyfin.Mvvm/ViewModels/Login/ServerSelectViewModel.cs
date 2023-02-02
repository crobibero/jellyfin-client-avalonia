using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels.Login;

/// <summary>
/// Server select view model.
/// </summary>
public partial class ServerSelectViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public ServerSelectViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        IStateService stateService)
        : base(navigationService, applicationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _stateService = stateService;

        ApplicationService.EnableCollectionSynchronization(Servers, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets the list of servers.
    /// </summary>
    public ObservableRangeCollection<ServerStateModel> Servers { get; } = new();

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);

        // if ConfigureAwait(continueOnCapturedContext: true), then there's no need to Dispatch
        ApplicationService.DispatchAsync(() =>
        {
            Servers.ReplaceRange(state.Servers);
        }).SafeFireAndForget();
    }

    [RelayCommand]
    private void AddServer()
    {
        _navigationService.NavigateToAddServerPage();
    }

    [RelayCommand]
    private async Task SelectServerAsync(ServerStateModel? server)
    {
        if (server is not null)
        {
            _stateService.SetServerState(server);
            await _stateStorageService.AddServerAsync(server).ConfigureAwait(false); // TODO: refactor, quick trick to save SelectedUser

            _navigationService.NavigateToUserSelectPage();
        }
    }

    [RelayCommand]
    private async Task DeleteServerAsync(ServerStateModel? server)
    {
        if (server is not null)
        {
            await _stateStorageService.RemoveServerAsync(server.Id).ConfigureAwait(true);
            await InitializeAsync().ConfigureAwait(true);
        }
    }
}
