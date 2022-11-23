namespace Jellyfin.Maui.Models;

/// <summary>
/// The user state model.
/// </summary>
public class UserStateModel
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    required public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the server id the user belongs to.
    /// </summary>
    required public Guid ServerId { get; init; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    required public string Name { get; set; }

    /// <summary>
    /// Gets or sets the user token.
    /// </summary>
    required public string Token { get; set; }
}
