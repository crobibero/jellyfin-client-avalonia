using Avalonia.Media.Imaging;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Decodes downloaded images into bitmaps.
/// </summary>
public interface IImageDecoder
{
    /// <summary>
    /// Decode an encoded image at its own size.
    /// </summary>
    /// <param name="encoded">The encoded image. It must stay unchanged until the returned task completes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The decoded bitmap.</returns>
    Task<Bitmap> DecodeAsync(ReadOnlyMemory<byte> encoded, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decode an encoded image, scale it to a width and blur it.
    /// </summary>
    /// <param name="encoded">The encoded image. It must stay unchanged until the returned task completes.</param>
    /// <param name="width">The width to scale the image to, keeping its aspect ratio.</param>
    /// <param name="sigma">The standard deviation of the blur, in pixels of the scaled image.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The decoded bitmap.</returns>
    Task<Bitmap> DecodeBlurredAsync(ReadOnlyMemory<byte> encoded, int width, float sigma, CancellationToken cancellationToken = default);
}
