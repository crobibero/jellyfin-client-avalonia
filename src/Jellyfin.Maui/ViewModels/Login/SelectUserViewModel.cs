using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels.Login;

/// <summary>
/// The select user view model.
/// </summary>
public class SelectUserViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public SelectUserViewModel(
        INavigationService navigationService,
        IStateStorageService stateStorageService,
        IStateService stateService)
        : base(navigationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _stateService = stateService;

        AddUserCommand = new RelayCommand(AddUser);
        SelectUserCommand = new RelayCommand(SelectUser);
    }

    /// <summary>
    /// Gets the add user command.
    /// </summary>
    public IRelayCommand AddUserCommand { get; }

    /// <summary>
    /// Gets the select user command.
    /// </summary>
    public IRelayCommand SelectUserCommand { get; }

    /// <summary>
    /// Gets the list of users.
    /// </summary>
    public ObservableCollection<UserStateModel> Users { get; } = new();

    /// <summary>
    /// Gets or sets the selected user.
    /// </summary>
    public UserStateModel? SelectedUser { get; set; }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);
        var serverState = _stateService.GetServerState();
        if (serverState is null)
        {
            _navigationService.NavigateToServerSelectPage();
            return;
        }

        Users.Clear();
        SelectedUser = null;
        foreach (var user in state.Users)
        {
            if (user.ServerId == serverState.Id)
            {
                Users.Add(user);
            }
        }
    }

    private void AddUser()
    {
        _navigationService.NavigateToLoginPage();
    }

    private void SelectUser()
    {
        if (SelectedUser is not null)
        {
            _stateService.SetUserState(SelectedUser);
            _navigationService.NavigateToLoginPage();
        }
    }
}
