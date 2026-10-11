using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// The add server view model.
/// </summary>
public partial class AddServerViewModel : BaseViewModel
{
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string? _serverUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    public AddServerViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        IStateService stateService,
        IAuthenticationService authenticationService)
        : base(navigationService, applicationService)
    {
        _stateStorageService = stateStorageService;
        _stateService = stateService;
        _authenticationService = authenticationService;
    }

    /// <inheritdoc />
    public override bool ShowToolbar => false;

    /// <inheritdoc />
    protected override ValueTask InitializeAsync(object? parameter)
    {
        if (parameter is string url)
        {
            ServerUrl = url;
        }

        return ValueTask.CompletedTask;
    }

    private bool CanConnect() => !string.IsNullOrWhiteSpace(ServerUrl);

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl))
        {
            ErrorMessage = Strings.Strings.Login_ServerUrlRequired;
            return;
        }

        Loading = true;
        ErrorMessage = null;
        try
        {
            var info = await _authenticationService.ResolveServerAsync(ServerUrl).ConfigureAwait(true);
            if (info is null)
            {
                ErrorMessage = Strings.Strings.Login_UnableToConnect;
                return;
            }

            var server = new ServerStateModel
            {
                Id = info.Id,
                Name = info.Name,
                Url = info.Url,
                Version = info.Version,
                LastUsed = DateTimeOffset.UtcNow,
            };

            await _stateStorageService.AddServerAsync(server).ConfigureAwait(true);
            _stateService.SetServerState(server);
            await NavigationService.NavigateAsync<SelectUserViewModel>().ConfigureAwait(true);
        }
        finally
        {
            Loading = false;
        }
    }
}
