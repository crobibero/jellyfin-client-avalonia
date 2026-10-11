namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// A user shown on the user select screen.
/// </summary>
/// <param name="Id">The user id.</param>
/// <param name="Name">The user name.</param>
/// <param name="ImageUrl">The user image url.</param>
/// <param name="HasStoredSession">Whether the user has a stored session.</param>
/// <param name="HasPassword">Whether the user has a password configured.</param>
public sealed record UserCardModel(Guid Id, string Name, string? ImageUrl, bool HasStoredSession, bool HasPassword)
{
    /// <summary>
    /// Gets the blurhash shown while the image loads.
    /// </summary>
    public string? ImageBlurHash { get; init; }

    /// <summary>
    /// Gets the initial shown when there is no image.
    /// </summary>
    public string Initial => string.IsNullOrEmpty(Name) ? "?" : Name[..1].ToUpperInvariant();

    /// <summary>
    /// Gets a value indicating whether the user has an image.
    /// </summary>
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);
}
