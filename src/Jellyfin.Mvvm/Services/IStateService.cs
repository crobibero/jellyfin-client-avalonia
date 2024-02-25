using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// State service interface.
/// </summary>
public interface IStateService
{
    /// <summary>
    /// Initialization.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asyncronous operation.</returns>
    ValueTask InitializeAsync();

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
    void SetServerState(ServerStateModel serverStateModel);

    /// <summary>
    /// Gets the current server.
    /// </summary>
    /// <returns>The server state model.</returns>
    ServerStateModel? GetServerState();

    /// <summary>
    /// Gets the current user.
    /// </summary>
    /// <returns>The user state model.</returns>
    UserStateModel? GetUserState();

    /// <summary>
    /// Sets the current user state.
    /// </summary>
    /// <param name="userStateModel">The user state model.</param>
    void SetUserState(UserStateModel userStateModel);

    /// <summary>
    /// Gets the current host.
    /// </summary>
    /// <returns>The current host.</returns>
    string GetHost();

    /// <summary>
    /// Sets the current host.
    /// </summary>
    /// <param name="host">The host.</param>
    void SetHost(string host);

    /// <summary>
    /// Gets the current token.
    /// </summary>
    /// <returns>The current token.</returns>
    string? GetToken();

    /// <summary>
    /// Clears the current state.
    /// </summary>
    void ClearState();
}
