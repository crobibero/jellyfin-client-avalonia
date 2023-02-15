using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// The select user view model.
/// </summary>
public partial class SelectUserViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public SelectUserViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        IStateService stateService)
        : base(navigationService, applicationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _stateService = stateService;

        ApplicationService.EnableCollectionSynchronization(Users, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets the list of users.
    /// </summary>
    public ObservableRangeCollection<UserStateModel> Users { get; } = new();

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(true);
        var serverState = _stateService.GetServerState();
        if (serverState is null)
        {
            _navigationService.NavigateToServerSelectPage();
            return;
        }

        Users.ReplaceRange(state.Users.Where(u => u.ServerId == serverState.Id));
    }

    [RelayCommand]
    private void AddUser()
    {
        _navigationService.NavigateToLoginPage();
    }

    [RelayCommand]
    private async Task SelectUserAsync(UserStateModel? user)
    {
        if (user is not null)
        {
            _stateService.SetUserState(user);
            await _stateStorageService.AddUserAsync(user).ConfigureAwait(false); // TODO: refactor, quick trick to save SelectedUser

            _navigationService.NavigateToLoginPage();
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(UserStateModel? user)
    {
        if (user is not null)
        {
            await _stateStorageService.RemoveUserAsync(user.Id, user.ServerId).ConfigureAwait(true);
            await InitializeAsync().ConfigureAwait(true);
        }
    }
}
