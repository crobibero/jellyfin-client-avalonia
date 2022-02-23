using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels.Login;

/// <summary>
/// Server select view model.
/// </summary>
public class ServerSelectViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public ServerSelectViewModel(
        INavigationService navigationService,
        IStateStorageService stateStorageService,
        IStateService stateService)
        : base(navigationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _stateService = stateService;

        AddServerCommand = new RelayCommand(AddServer);
        SelectServerCommand = new RelayCommand<ServerStateModel>(SelectServer);
    }

    /// <summary>
    /// Gets the add server command.
    /// </summary>
    public IRelayCommand AddServerCommand { get; }

    /// <summary>
    /// Gets the select server command.
    /// </summary>
    public IRelayCommand<ServerStateModel> SelectServerCommand { get; }

    /// <summary>
    /// Gets the list of servers.
    /// </summary>
    public ObservableRangeCollection<ServerStateModel> Servers { get; } = new();

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);
        Servers.ReplaceRange(state.Servers);
    }

    private void AddServer()
    {
        _navigationService.NavigateToAddServerPage();
    }

    private void SelectServer(ServerStateModel? server)
    {
        if (server is not null)
        {
            _stateService.SetServerState(server);
            _navigationService.NavigateToUserSelectPage();
        }
    }
}
