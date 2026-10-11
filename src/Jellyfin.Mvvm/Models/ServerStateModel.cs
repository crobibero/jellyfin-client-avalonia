namespace Jellyfin.Mvvm.Models;

/// <summary>
/// The server state model.
/// </summary>
public class ServerStateModel
{
    /// <summary>
    /// Gets the server id.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the server url.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the server version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the last time the server was used.
    /// </summary>
    public DateTimeOffset? LastUsed { get; set; }
}
