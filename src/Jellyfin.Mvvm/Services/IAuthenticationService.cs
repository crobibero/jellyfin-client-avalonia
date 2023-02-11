namespace Jellyfin.Mvvm.Services;

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
    /// Initialize the QuickConnect process.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The code.</returns>
    ValueTask<string?> InitializeQuickConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Test the pending QuickConnect authentication request.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>QuickConnect status.</returns>
    ValueTask<bool?> TestQuickConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate using QuickConnect.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication status.</returns>
    ValueTask<(bool Status, string? ErrorMessage)> AuthenticateWithQuickConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether QuickConnect is enabled.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Whether QuickConnect is enabled.</returns>
    ValueTask<bool> IsQuickConnectEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Log out of current session.
    /// </summary>
    void Logout();

    /// <summary>
    /// Test whether the client is currently authenticated.
    /// </summary>
    /// <param name="host">The server host.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication status.</returns>
    ValueTask<bool> IsAuthenticatedAsync(
        string host,
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Test whether client is currently authenticated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication status.</returns>
    ValueTask<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default);
}
