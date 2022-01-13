using Jellyfin.Maui.Models;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <summary>
/// State service interface.
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
    UserDto GetCurrentUser();

    /// <summary>
    /// Sets the current server.
    /// </summary>
    /// <param name="serverStateModel">The server state model.</param>
    void SetServer(ServerStateModel serverStateModel);

    /// <summary>
    /// Gets the current server.
    /// </summary>
    /// <returns>The server state model.</returns>
    ServerStateModel GetServer();

    /// <summary>
    /// Gets the current host.
    /// </summary>
    /// <returns>The current host.</returns>
    string GetHost();

    /// <summary>
    /// Clears the current state.
    /// </summary>
    void ClearState();
}
