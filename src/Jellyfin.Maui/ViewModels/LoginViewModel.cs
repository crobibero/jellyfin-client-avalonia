using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Login view model.
/// </summary>
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    private string? _serverUrl = "https://demo.jellyfin.org/stable";
    private string? _username = "demo";
    private string? _password;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public LoginViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService)
    {
        _authenticationService = authenticationService;
        _navigationService = navigationService;

        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    /// <summary>
    /// Gets or sets the server url.
    /// </summary>
    public string? ServerUrl
    {
        get => _serverUrl;
        set => SetProperty(ref _serverUrl, value);
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
    public override void Initialize()
    {
    }

    private async Task LoginAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(ServerUrl))
            {
                ErrorMessage = "Server URL is required";
                return;
            }

            if (string.IsNullOrEmpty(Username))
            {
                ErrorMessage = "Username is required";
                return;
            }

            var (status, errorMessage) = await _authenticationService.AuthenticateAsync(
                    ServerUrl,
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
