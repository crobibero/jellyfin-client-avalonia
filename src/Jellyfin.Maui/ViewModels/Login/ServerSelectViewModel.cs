using System.Collections.ObjectModel;
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
        SelectServerCommand = new RelayCommand(SelectServer);
    }

    /// <summary>
    /// Gets the add server command.
    /// </summary>
    public IRelayCommand AddServerCommand { get; }

    /// <summary>
    /// Gets the select server command.
    /// </summary>
    public IRelayCommand SelectServerCommand { get; }

    /// <summary>
    /// Gets the list of servers.
    /// </summary>
    public ObservableCollection<ServerStateModel> Servers { get; } = new();

    /// <summary>
    /// Gets or sets the selected server.
    /// </summary>
    public ServerStateModel? SelectedServer { get; set; }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);
        Servers.Clear();
        SelectedServer = null;
        foreach (var storedServer in state.Servers)
        {
            Servers.Add(storedServer);
        }
    }

    private void AddServer()
    {
        _navigationService.NavigateToAddServerPage();
    }

    private void SelectServer()
    {
        if (SelectedServer is not null)
        {
            _stateService.SetServer(SelectedServer);
            _navigationService.NavigateToLoginPage();
        }
    }
}
