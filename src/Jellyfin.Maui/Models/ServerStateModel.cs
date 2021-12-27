namespace Jellyfin.Maui.Models;

/// <summary>
/// The server state model.
/// </summary>
public class ServerStateModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerStateModel"/> class.
    /// </summary>
    /// <param name="id">The server id.</param>
    /// <param name="name">The server name.</param>
    /// <param name="url">The server url.</param>
    public ServerStateModel(Guid id, string name, string url)
    {
        Id = id;
        Name = name;
        Url = url;
    }

    /// <summary>
    /// Gets the server id.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the server name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the server url.
    /// </summary>
    public string Url { get; init; }
}
