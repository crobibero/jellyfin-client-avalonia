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

    /// <summary>
    /// Gets or sets the last selected ServerId.
    /// </summary>
    public Guid? SelectedServerId { get; set; }

    /// <summary>
    /// Gets or sets the last selected UserId.
    /// </summary>
    public Guid? SelectedUserId { get; set; }
}
