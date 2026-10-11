using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Resolve a user entered address to a server, trying common schemes and ports.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The server, or <c>null</c> when no server responded.</returns>
    ValueTask<ServerInfoModel?> ResolveServerAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public users of the current server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The public users.</returns>
    ValueTask<IReadOnlyList<UserDto>> GetPublicUsersAsync(CancellationToken cancellationToken = default);

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
    /// Log out of the current session.
    /// </summary>
    /// <param name="revokeToken">Whether to revoke the access token on the server.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask LogoutAsync(bool revokeToken = false);

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
