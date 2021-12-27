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

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    public ServerSelectViewModel(
        INavigationService navigationService,
        IStateStorageService stateStorageService)
        : base(navigationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;

        AddServerCommand = new RelayCommand(AddServer);
    }

    /// <summary>
    /// Gets the add server command.
    /// </summary>
    public IRelayCommand AddServerCommand { get; }

    /// <summary>
    /// Gets the list of servers.
    /// </summary>
    public ObservableCollection<ServerStateModel> Servers { get; } = new();

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);
        foreach (var storedServer in state.Servers)
        {
            Servers.Add(storedServer);
        }
    }

    private void AddServer()
    {
        _navigationService.NavigateToAddServerPage();
    }
}
