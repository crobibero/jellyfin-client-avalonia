using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels.Login;

/// <summary>
/// Login view model.
/// </summary>
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;
    private readonly IStateService _stateService;
    private readonly IStateStorageService _stateStorageService;

    // _serverUrl is set on initialization.
    private string _serverUrl = null!;
    private Guid _serverId;
    private string _serverName = null!;
    private string? _username;
    private string? _password;
    private bool _rememberMe = true;
    private string? _errorMessage;
    private bool _quickConnectAvailable;
    private string? _quickConnectCode;

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

        LoginCommand = new AsyncRelayCommand(LoginAsync);
        LoginQuickConnectCommand = new AsyncRelayCommand(LoginWithQuickConnectAsync);
    }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string? Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the client should remember the user.
    /// </summary>
    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether QuickConnect is available.
    /// </summary>
    public bool QuickConnectAvailable
    {
        get => _quickConnectAvailable;
        set => SetProperty(ref _quickConnectAvailable, value);
    }

    /// <summary>
    /// Gets or sets the QuickConnect code.
    /// </summary>
    public string? QuickConnectCode
    {
        get => _quickConnectCode;
        set => SetProperty(ref _quickConnectCode, value);
    }

    /// <summary>
    /// Gets the login command.
    /// </summary>
    public IAsyncRelayCommand LoginCommand { get; }

    /// <summary>
    /// Gets the login with QuickConnect command.
    /// </summary>
    public IAsyncRelayCommand LoginQuickConnectCommand { get; }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        var server = _stateService.GetServerState();
        if (server is null)
        {
            _navigationService.NavigateToServerSelectPage();
            return;
        }

        _serverId = server.Id;
        _serverUrl = server.Url;
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
            }
        }

        QuickConnectAvailable = await _authenticationService.IsQuickConnectEnabledAsync().ConfigureAwait(false);
    }

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
                    _serverUrl,
                    Username,
                    Password)
                .ConfigureAwait(false);
            if (status)
            {
                if (_rememberMe)
                {
                    var user = _stateService.GetCurrentUser();
                    await _stateStorageService.AddUserAsync(new UserStateModel(user.Id, _serverId, user.Name, _stateService.GetToken()))
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

    private async Task LoginWithQuickConnectAsync()
    {
        var code = await _authenticationService.InitializeQuickConnectAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(code))
        {
            ErrorMessage = "Unable to initialize QuickConnect";
        }
        else
        {
            QuickConnectCode = code;
        }

        try
        {
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
                        await _stateStorageService.AddUserAsync(new UserStateModel(user.Id, _serverId, user.Name, _stateService.GetToken()))
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
