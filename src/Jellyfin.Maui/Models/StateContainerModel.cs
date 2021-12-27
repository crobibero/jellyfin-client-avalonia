using System.Collections.ObjectModel;

namespace Jellyfin.Maui.Models;

/// <summary>
/// The state container.
/// </summary>
public class StateContainerModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StateContainerModel"/> class.
    /// </summary>
    public StateContainerModel()
    {
        Servers = new Collection<ServerStateModel>();
        Users = new Collection<UserStateModel>();
    }

    /// <summary>
    /// Gets the collection of servers.
    /// </summary>
    public Collection<ServerStateModel> Servers { get; init; }

    /// <summary>
    /// Gets the collection of users.
    /// </summary>
    public Collection<UserStateModel> Users { get; init; }

    // TODO store current user & server.
}
