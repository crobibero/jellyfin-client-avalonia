namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Computes blurhashes for images the server has none for, such as user images.
/// </summary>
public interface IImageBlurHashService
{
    /// <summary>
    /// Loads an image and computes its blurhash.
    /// </summary>
    /// <param name="imageUrl">The image url.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The blurhash, or <c>null</c> when the image could not be loaded.</returns>
    Task<string?> GetBlurHashAsync(string imageUrl, CancellationToken cancellationToken = default);
}
