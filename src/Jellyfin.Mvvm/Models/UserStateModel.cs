namespace Jellyfin.Mvvm.Models;

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
    /// Gets or sets the user token, or <c>null</c> when the user isn't signed in.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the primary image tag.
    /// </summary>
    public string? ImageTag { get; set; }

    /// <summary>
    /// Gets or sets the blurhash of the primary image. The server sends none for users, so the client computes it.
    /// </summary>
    public string? ImageBlurHash { get; set; }

    /// <summary>
    /// Gets or sets the last time the user signed in.
    /// </summary>
    public DateTimeOffset? LastUsed { get; set; }
}
