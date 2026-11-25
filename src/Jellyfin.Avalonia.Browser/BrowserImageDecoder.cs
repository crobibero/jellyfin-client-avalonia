using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Jellyfin.Avalonia.Services;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// Decodes images in web workers.
/// </summary>
/// <remarks>
/// WebAssembly runs the app, layout and rendering on the main thread, and Skia decoding there stalls scrolling.
/// Workers decode with the browser's own decoders, so the main thread only copies the pixels into a bitmap.
/// Images the workers can't decode fall back to Skia.
/// </remarks>
[SupportedOSPlatform("browser")]
internal sealed class BrowserImageDecoder : IImageDecoder
{
    private static readonly Vector _dpi = new(96, 96);

    private readonly bool _isSupported = BrowserInterop.IsImageDecoderSupported();

    /// <inheritdoc />
    public Task<Bitmap> DecodeAsync(ReadOnlyMemory<byte> encoded, CancellationToken cancellationToken = default)
        => DecodeAsync(encoded, 0, 0, static encoded => SkiaImageDecoder.Decode(encoded), cancellationToken);

    /// <inheritdoc />
    public Task<Bitmap> DecodeBlurredAsync(ReadOnlyMemory<byte> encoded, int width, float sigma, CancellationToken cancellationToken = default)
        => DecodeAsync(encoded, width, sigma, encoded => SkiaImageDecoder.DecodeBlurred(encoded, width, sigma), cancellationToken);

    private async Task<Bitmap> DecodeAsync(
        ReadOnlyMemory<byte> encoded,
        int width,
        float sigma,
        Func<ReadOnlyMemory<byte>, Bitmap> fallback,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_isSupported)
        {
            return fallback(encoded);
        }

        if (!MemoryMarshal.TryGetArray(encoded, out var segment))
        {
            segment = encoded.ToArray();
        }

        try
        {
            using var image = await BrowserInterop.DecodeImage(segment, width, sigma).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();
            return CreateBitmap(image);
        }
        catch (JSException)
        {
            return fallback(encoded);
        }
    }

    private static unsafe Bitmap CreateBitmap(JSObject image)
    {
        var size = new PixelSize(image.GetPropertyAsInt32("width"), image.GetPropertyAsInt32("height"));
        var alphaFormat = image.GetPropertyAsBoolean("opaque") ? AlphaFormat.Opaque : AlphaFormat.Premul;
        var stride = size.Width * 4;
        var length = stride * size.Height;

        // Bitmap copies the pixels, so the buffer is freed straight after and never reaches the large object heap.
        var pixels = NativeMemory.Alloc((nuint)length);
        try
        {
            BrowserInterop.CopyImagePixels(image, new Span<byte>(pixels, length));
            return new Bitmap(PixelFormat.Rgba8888, alphaFormat, (nint)pixels, size, _dpi, stride);
        }
        finally
        {
            NativeMemory.Free(pixels);
        }
    }
}
