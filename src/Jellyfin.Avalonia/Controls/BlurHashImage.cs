using System.Buffers;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Jellyfin.Mvvm.Imaging;

namespace Jellyfin.Avalonia.Controls;

/// <summary>
/// Draws a blurhash placeholder stretched over the control.
/// </summary>
/// <remarks>
/// Changing the hash only invalidates rendering. Item repeaters rebind recycled cards during layout, and an image
/// source change there invalidates measure and causes a layout loop. An image brush avoids that too, but Skia renders
/// every image brush into a new intermediate surface on every frame, which floods the finalizer and garbage collector
/// while a grid of cards scrolls. This draws a cached bitmap directly.
/// </remarks>
public sealed class BlurHashImage : Control
{
    /// <summary>
    /// Defines the <see cref="Hash"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> HashProperty =
        AvaloniaProperty.Register<BlurHashImage, string?>(nameof(Hash));

    // Blurhashes only hold a few low frequency components, so a small bitmap scales up without visible loss.
    private const int Size = 32;

    // Recycled cards rebind constantly while scrolling, so decoded placeholders are reused by hash.
    private const int CacheSize = 512;

    // Only touched on the UI thread.
    private static readonly Dictionary<string, LinkedListNode<(string Hash, Bitmap Bitmap)>> _cache = new(StringComparer.Ordinal);
    private static readonly LinkedList<(string Hash, Bitmap Bitmap)> _lru = new();

    private static readonly RenderOptions _renderOptions = new() { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality };

    private Bitmap? _bitmap;

    static BlurHashImage()
    {
        AffectsRender<BlurHashImage>(HashProperty);
        IsHitTestVisibleProperty.OverrideDefaultValue<BlurHashImage>(false);
    }

    /// <summary>
    /// Gets or sets the blurhash.
    /// </summary>
    public string? Hash
    {
        get => GetValue(HashProperty);
        set => SetValue(HashProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (_bitmap is not { } bitmap)
        {
            return;
        }

        // The hash is decoded to a square, so stretching it to the control restores the image's own proportions.
        using (context.PushRenderOptions(_renderOptions))
        {
            context.DrawImage(bitmap, new Rect(bitmap.Size), new Rect(Bounds.Size));
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == HashProperty)
        {
            _bitmap = GetBitmap(Hash);
        }
    }

    private static Bitmap? GetBitmap(string? hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return null;
        }

        if (_cache.TryGetValue(hash, out var node))
        {
            _lru.Remove(node);
            _lru.AddFirst(node);
            return node.Value.Bitmap;
        }

        var bitmap = Decode(hash);
        if (bitmap is null)
        {
            return null;
        }

        _cache[hash] = _lru.AddFirst((hash, bitmap));
        if (_lru.Count > CacheSize)
        {
            // Evicted bitmaps may still be shown by a control, so they are left to the garbage collector.
            _cache.Remove(_lru.Last!.Value.Hash);
            _lru.RemoveLast();
        }

        return bitmap;
    }

    private static Bitmap? Decode(string hash)
    {
        const int RowLength = Size * 4;
        var pixels = ArrayPool<byte>.Shared.Rent(Size * RowLength);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            if (!BlurHashDecoder.TryDecode(hash, Size, Size, pixels))
            {
                return null;
            }

            // An immutable bitmap is uploaded to the GPU once. A writeable one is snapshotted on every draw.
            return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Opaque, handle.AddrOfPinnedObject(), new PixelSize(Size, Size), new Vector(96, 96), RowLength);
        }
        finally
        {
            handle.Free();
            ArrayPool<byte>.Shared.Return(pixels);
        }
    }
}
