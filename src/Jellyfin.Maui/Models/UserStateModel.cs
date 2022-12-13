namespace Jellyfin.Maui.Models;

/// <summary>
/// The user state model.
/// </summary>
public class UserStateModel
{
    /// <summary>
    /// Gets the user id.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the server id the user belongs to.
    /// </summary>
    public required Guid ServerId { get; init; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the user token.
    /// </summary>
    public required string Token { get; set; }
}
