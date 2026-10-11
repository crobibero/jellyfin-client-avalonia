using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Discovers Jellyfin servers on the local network.
/// </summary>
public interface IServerDiscoveryService
{
    /// <summary>
    /// Discover servers.
    /// </summary>
    /// <param name="timeout">How long to listen for responses.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered servers.</returns>
    IAsyncEnumerable<ServerDiscoveryResponse> DiscoverAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}
