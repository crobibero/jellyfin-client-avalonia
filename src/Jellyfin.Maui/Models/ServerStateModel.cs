namespace Jellyfin.Maui.Models;

/// <summary>
/// The server state model.
/// </summary>
public class ServerStateModel
{
    /// <summary>
    /// Gets or sets the server id.
    /// </summary>
    required public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    required public string Name { get; set; }

    /// <summary>
    /// Gets or sets the server url.
    /// </summary>
    required public string Url { get; set; }
}
