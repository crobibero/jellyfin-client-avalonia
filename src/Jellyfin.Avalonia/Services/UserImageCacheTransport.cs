using AsyncImageLoader.Core.Caching;
using AsyncImageLoader.Core.Pipeline;
using AsyncImageLoader.Core.Transport;
using Avalonia;
using Avalonia.Media.Imaging;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Keeps scaled down user images on disk so the user picker and toolbar avatar show without a download.
/// </summary>
/// <remarks>
/// The url holds the image tag, so a changed image gets a new entry.
/// </remarks>
public sealed class UserImageCacheTransport : IImageTransport
{
    // The server always sends the original user image. This covers the largest avatar on a high density screen.
    private const int MaxSize = 256;

    private readonly IImageTransport _inner;
    private readonly DiskImageByteCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserImageCacheTransport"/> class.
    /// </summary>
    /// <param name="inner">The transport that downloads images.</param>
    /// <param name="cacheDirectory">The directory to keep user images in.</param>
    public UserImageCacheTransport(IImageTransport inner, string cacheDirectory)
    {
        _inner = inner;
        _cache = new DiskImageByteCache(cacheDirectory);
    }

    /// <inheritdoc />
    public async Task<Stream?> GetAsync(ImageLoadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!IsUserImage(request.Source))
        {
            return await _inner.GetAsync(request, cancellationToken).ConfigureAwait(false);
        }

        if (await _cache.GetAsync(request.Source, cancellationToken).ConfigureAwait(false) is { } cached)
        {
            return cached;
        }

        var response = await _inner.GetAsync(request, cancellationToken).ConfigureAwait(false);
        if (response is null)
        {
            return null;
        }

        var buffered = new MemoryStream();
        await using (response.ConfigureAwait(false))
        {
            await response.CopyToAsync(buffered, cancellationToken).ConfigureAwait(false);
        }

        buffered.Position = 0;
        buffered = ScaleDown(buffered);
        try
        {
            await _cache.SetAsync(request.Source, buffered, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Still show the image when the cache can't be written.
        }

        buffered.Position = 0;
        return buffered;
    }

    private static MemoryStream ScaleDown(MemoryStream encoded)
    {
        using var original = new Bitmap(encoded);
        var size = original.PixelSize;
        if (Math.Max(size.Width, size.Height) <= MaxSize)
        {
            encoded.Position = 0;
            return encoded;
        }

        var scale = (double)MaxSize / Math.Max(size.Width, size.Height);
        var scaledSize = new PixelSize(Math.Max(1, (int)(size.Width * scale)), Math.Max(1, (int)(size.Height * scale)));
        using var scaled = original.CreateScaledBitmap(scaledSize, BitmapInterpolationMode.HighQuality);

        // Png keeps transparent avatars transparent.
        var result = new MemoryStream();
        scaled.Save(result, PngBitmapEncoderOptions.Default);
        result.Position = 0;
        encoded.Dispose();
        return result;
    }

    private static bool IsUserImage(string source)
        => Uri.TryCreate(source, UriKind.Absolute, out var uri)
           && uri.AbsolutePath.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
           && uri.AbsolutePath.Contains("/Images/", StringComparison.OrdinalIgnoreCase);
}
