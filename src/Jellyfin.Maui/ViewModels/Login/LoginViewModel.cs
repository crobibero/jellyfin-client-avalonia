using CommunityToolkit.Mvvm.Input;
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

    // _serverUrl is set on initialization.
    private string _serverUrl = null!;
    private string? _serverName;
    private string? _username;
    private string? _password;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public LoginViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        IStateService stateService)
        : base(navigationService)
    {
        _authenticationService = authenticationService;
        _navigationService = navigationService;
        _stateService = stateService;

        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string? ServerName
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
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets the login command.
    /// </summary>
    public IAsyncRelayCommand LoginCommand { get; }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
    {
        try
        {
            var server = _stateService.GetServer();
            _serverUrl = server.Url;
            ServerName = server.Name;
        }
        catch (InvalidOperationException)
        {
            _navigationService.NavigateToServerSelectPage();
        }

        return ValueTask.CompletedTask;
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
                    Password,
                    ViewModelCancellationToken)
                .ConfigureAwait(false);
            if (status)
            {
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
}
