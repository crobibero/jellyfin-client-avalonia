using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.ViewModels
{
    /// <summary>
    /// The login view model.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authenticationService;

        private string? serverUrl;
        private string? username;
        private string? password;
        private string? errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
        public LoginViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        /// <summary>
        /// Gets or sets the server url.
        /// </summary>
        public string? ServerUrl
        {
            get => serverUrl;
            set => SetProperty(ref serverUrl, value);
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string? Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string? Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        /// <summary>
        /// Gets the login command.
        /// </summary>
        public IAsyncRelayCommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            try
            {
                var (status, errorMessage) = await _authenticationService.AuthenticateAsync(
                    ServerUrl,
                    Username,
                    Password)
                    .ConfigureAwait(false);
                if (status)
                {
                    // TODO navigate
                    ErrorMessage = "success";
                }
                else
                {
                    ErrorMessage = errorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unknown error occurred.";
            }
        }
    }
}
