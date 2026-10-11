namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A server found on the local network.
/// </summary>
public sealed record ServerDiscoveryResponse
{
    /// <summary>
    /// Gets the server address.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Gets the server id.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the server name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the endpoint address.
    /// </summary>
    public string? EndpointAddress { get; init; }
}
