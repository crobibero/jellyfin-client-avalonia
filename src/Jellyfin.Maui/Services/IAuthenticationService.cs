namespace Jellyfin.Maui.Services;

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticate against the host using the provided credentials.
    /// </summary>
    /// <param name="host">The server host.</param>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication response.</returns>
    ValueTask<(bool Status, string? ErrorMessage)> AuthenticateAsync(
        string host,
        string username,
        string? password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log out of current session.
    /// </summary>
    void Logout();

    /// <summary>
    /// Test whether client is currently authenticated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication status.</returns>
    ValueTask<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}
