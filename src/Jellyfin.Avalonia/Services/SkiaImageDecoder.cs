using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Decodes images with Skia on the calling thread.
/// </summary>
public sealed class SkiaImageDecoder : IImageDecoder
{
    private static readonly Vector _dpi = new(96, 96);

    /// <inheritdoc />
    public Task<Bitmap> DecodeAsync(ReadOnlyMemory<byte> encoded, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Decode(encoded));
    }

    /// <inheritdoc />
    public Task<Bitmap> DecodeBlurredAsync(ReadOnlyMemory<byte> encoded, int width, float sigma, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(DecodeBlurred(encoded, width, sigma));
    }

    /// <summary>
    /// Decode an encoded image at its own size.
    /// </summary>
    /// <param name="encoded">The encoded image.</param>
    /// <returns>The decoded bitmap.</returns>
    public static Bitmap Decode(ReadOnlyMemory<byte> encoded)
    {
        // Bitmap copies what it needs while decoding, so the stream can wrap the caller's buffer.
        using var stream = MemoryMarshal.TryGetArray(encoded, out var segment)
            ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new MemoryStream(encoded.ToArray(), writable: false);
        return new Bitmap(stream);
    }

    /// <summary>
    /// Decode an encoded image, scale it to a width and blur it.
    /// </summary>
    /// <param name="encoded">The encoded image.</param>
    /// <param name="width">The width to scale the image to, keeping its aspect ratio.</param>
    /// <param name="sigma">The standard deviation of the blur, in pixels of the scaled image.</param>
    /// <returns>The decoded bitmap.</returns>
    public static Bitmap DecodeBlurred(ReadOnlyMemory<byte> encoded, int width, float sigma)
    {
        using var data = SKData.CreateCopy(encoded.Span);
        using var codec = SKCodec.Create(data) ?? throw new InvalidDataException("The image format is not supported.");

        // JPEG decodes straight to an eighth, quarter or half of its size for a fraction of the full cost.
        var info = codec.Info;
        var decodedSize = codec.GetScaledDimensions(Math.Min(1f, (float)width / info.Width));
        var alphaType = info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul;
        using var decoded = SKBitmap.Decode(codec, new SKImageInfo(decodedSize.Width, decodedSize.Height, SKImageInfo.PlatformColorType, alphaType))
            ?? throw new InvalidDataException("The image could not be decoded.");
        using var image = SKImage.FromBitmap(decoded);

        var height = Math.Max(1, (int)Math.Round((double)info.Height * width / info.Width));
        using var target = new SKBitmap(new SKImageInfo(width, height, SKImageInfo.PlatformColorType, alphaType));
        using (var canvas = new SKCanvas(target))
        using (var blur = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp))
        using (var paint = new SKPaint { ImageFilter = blur })
        {
            // Clamping repeats the edge pixels into the blur, so the edges don't fade to transparent.
            canvas.DrawImage(image, SKRect.Create(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
        }

        var format = target.ColorType == SKColorType.Rgba8888 ? PixelFormat.Rgba8888 : PixelFormat.Bgra8888;
        return new Bitmap(
            format,
            alphaType == SKAlphaType.Opaque ? AlphaFormat.Opaque : AlphaFormat.Premul,
            target.GetPixels(),
            new PixelSize(width, height),
            _dpi,
            target.RowBytes);
    }
}
