using System.Runtime.InteropServices;
using AsyncImageLoader;
using AsyncImageLoader.Core.Pipeline;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Jellyfin.Mvvm.Imaging;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Computes blurhashes from images loaded through the shared image loader.
/// </summary>
public sealed class ImageBlurHashService : IImageBlurHashService
{
    // Blurhashes only hold a few low frequency components, so a small sample loses nothing.
    private const int SampleSize = 32;

    /// <inheritdoc />
    public async Task<string?> GetBlurHashAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        // Goes through the shared loader, so it reuses the image the view is already loading.
        using var lease = await ImageLoader.AsyncImageLoader.LoadAsync(new ImageLoadRequest(imageUrl), cancellationToken).ConfigureAwait(false);
        if (lease?.Image is not Bitmap bitmap)
        {
            return null;
        }

        using var sample = bitmap.CreateScaledBitmap(new PixelSize(SampleSize, SampleSize), BitmapInterpolationMode.MediumQuality);
        using var target = new WriteableBitmap(new PixelSize(SampleSize, SampleSize), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using var buffer = target.Lock();
        sample.CopyPixels(buffer);

        const int RowLength = SampleSize * 4;
        var pixels = new byte[SampleSize * RowLength];
        for (var row = 0; row < SampleSize; row++)
        {
            Marshal.Copy(buffer.Address + (row * buffer.RowBytes), pixels, row * RowLength, RowLength);
        }

        return BlurHashEncoder.Encode(pixels, SampleSize, SampleSize);
    }
}
