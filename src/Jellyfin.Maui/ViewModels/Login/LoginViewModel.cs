using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels.Login;

/// <summary>
/// Login view model.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;
    private readonly IStateService _stateService;
    private readonly IStateStorageService _stateStorageService;

    // _serverUrl is set on initialization.
    [ObservableProperty]
    private string _serverUrl = null!;

    [ObservableProperty]
    private bool _loading = true;

    [ObservableProperty]
    private Guid _serverId;

    [ObservableProperty]
    private string _serverName = null!;

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private bool _rememberMe = true;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _quickConnectAvailable;

    [ObservableProperty]
    private string? _quickConnectCode;

    [ObservableProperty]
    private bool _checkingQuickConnectAvailablity;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    public LoginViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        IStateService stateService,
        IStateStorageService stateStorageService)
        : base(navigationService)
    {
        _authenticationService = authenticationService;
        _navigationService = navigationService;
        _stateService = stateService;
        _stateStorageService = stateStorageService;
    }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var server = _stateService.GetServerState();
        if (server is null)
        {
            _navigationService.NavigateToServerSelectPage();
            return;
        }

        ServerId = server.Id;
        ServerUrl = server.Url;
        ServerName = server.Name;

        var user = _stateService.GetUserState();
        if (user is not null)
        {
            Username = user.Name;
            var status = await _authenticationService.IsAuthenticatedAsync(_serverUrl, user.Token)
                .ConfigureAwait(false);
            if (status)
            {
                _navigationService.NavigateHome();
                return;
            }
        }

        Loading = false;

        CheckQuickConnectAvailablity().SafeFireAndForget();
    }

    private async Task CheckQuickConnectAvailablity()
    {
        CheckingQuickConnectAvailablity = true;

        await Task.Delay(2_000).ConfigureAwait(false);  // mock slow network
        QuickConnectAvailable = await _authenticationService.IsQuickConnectEnabledAsync().ConfigureAwait(false);

        CheckingQuickConnectAvailablity = false;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Username))
            {
                ErrorMessage = "Username is required";
                return;
            }

            var (status, errorMessage) = await _authenticationService.AuthenticateAsync(
                    ServerUrl,
                    Username,
                    Password)
                .ConfigureAwait(false);
            if (status)
            {
                if (_rememberMe)
                {
                    var user = _stateService.GetCurrentUser();
                    await _stateStorageService.AddUserAsync(new UserStateModel
                        {
                            Id = user.Id,
                            ServerId = _serverId,
                            Name = user.Name,
                            Token = _stateService.GetToken()
                        })
                        .ConfigureAwait(false);
                }

                _navigationService.NavigateHome();
            }
            else
            {
                ErrorMessage = errorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unknown error occurred.\n" + ex.Message;
        }
    }

    [RelayCommand]
    private async Task LoginWithQuickConnectAsync()
    {
        try
        {
            var code = await _authenticationService.InitializeQuickConnectAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(code))
            {
                ErrorMessage = "Unable to initialize QuickConnect";
                return;
            }
            else
            {
                QuickConnectCode = code;
            }

            bool? authenticated;

            do
            {
                await Task.Delay(3_000).ConfigureAwait(false);
                authenticated = await _authenticationService.TestQuickConnectAsync().ConfigureAwait(false);
            }
            while (authenticated == false);

            QuickConnectCode = null;
            if (authenticated == true)
            {
                var (status, errorMessage) = await _authenticationService.AuthenticateWithQuickConnectAsync()
                    .ConfigureAwait(false);

                if (status)
                {
                    if (_rememberMe)
                    {
                        var user = _stateService.GetCurrentUser();
                        await _stateStorageService.AddUserAsync(new UserStateModel
                            {
                                Id = user.Id,
                                ServerId = _serverId,
                                Name = user.Name,
                                Token = _stateService.GetToken()
                            })
                            .ConfigureAwait(false);
                    }

                    _navigationService.NavigateHome();
                }
                else
                {
                    ErrorMessage = errorMessage;
                }
            }
            else
            {
                ErrorMessage = "Unable to connect with QuickConnect";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unknown error occurred.\n" + ex.Message;
        }
    }
}
