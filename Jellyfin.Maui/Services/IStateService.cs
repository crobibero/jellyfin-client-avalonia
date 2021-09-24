using System;
using Jellyfin.Maui.Models;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services
{
    /// <summary>
    /// The <see cref="IStateService"/>.
    /// </summary>
    public interface IStateService
    {
        /// <summary>
        /// Store the authentication response in the state.
        /// </summary>
        /// <param name="host">The server host.</param>
        /// <param name="authenticationResult">The authentication result.</param>
        void SetAuthenticationResponse(string host, AuthenticationResult authenticationResult);

        /// <summary>
        /// Store the user in the state.
        /// </summary>
        /// <param name="userDto">The user dto.</param>
        void SetUser(UserDto userDto);

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>The <see cref="UserDto"/>.</returns>
        UserDto GetUser();

        /// <summary>
        /// Gets the current userId.
        /// </summary>
        /// <returns>The current user id.</returns>
        Guid GetUserId();

        /// <summary>
        /// Gets the current host.
        /// </summary>
        /// <returns>The current host.</returns>
        string GetHost();

        /// <summary>
        /// Gets the current state.
        /// </summary>
        /// <returns>The <see cref="StateModel"/>.</returns>
        StateModel GetState();

        /// <summary>
        /// Clears the current state.
        /// </summary>
        void ClearState();
    }
}