namespace Jellyfin.Maui.Models;

/// <summary>
/// The user state model.
/// </summary>
public class UserStateModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserStateModel"/> class.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <param name="serverId">The server id the user belongs to.</param>
    /// <param name="name">The user name.</param>
    /// <param name="token">The user token.</param>
    public UserStateModel(Guid id, Guid serverId, string name, string token)
    {
        Id = id;
        ServerId = serverId;
        Name = name;
        Token = token;
    }

    /// <summary>
    /// Gets the user id.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the server id the user belongs to.
    /// </summary>
    public Guid ServerId { get; init; }

    /// <summary>
    /// Gets the user name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the user token.
    /// </summary>
    public string Token { get; init; }
}
