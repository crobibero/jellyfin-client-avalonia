using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Jellyfin.Avalonia.Services;

namespace Jellyfin.Avalonia.LibVlc;

/// <summary>
/// Renders frames from a <see cref="VideoFrameBuffer"/>.
/// </summary>
public sealed class LibVlcVideoView : Control, IVideoView, IDisposable
{
    private readonly VideoFrameBuffer _frames;
    private WriteableBitmap? _bitmap;
    private int _updatePending;
    private long _lastSequence = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibVlcVideoView"/> class.
    /// </summary>
    /// <param name="frames">The frame buffer.</param>
    public LibVlcVideoView(VideoFrameBuffer frames)
    {
        _frames = frames;
        ClipToBounds = true;
    }

    /// <inheritdoc />
    public Stretch VideoStretch { get; set; } = Stretch.Uniform;

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));
        if (_bitmap is null)
        {
            return;
        }

        var source = new Rect(_bitmap.Size);
        var destination = GetDestination(_bitmap.Size, Bounds.Size, VideoStretch);
        context.DrawImage(_bitmap, source, destination);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _frames.FrameAvailable += OnFrameAvailable;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _frames.FrameAvailable -= OnFrameAvailable;
        _bitmap?.Dispose();
        _bitmap = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _bitmap?.Dispose();
        _bitmap = null;
    }

    private static Rect GetDestination(Size image, Size bounds, Stretch stretch)
    {
        if (image.Width <= 0 || image.Height <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return new Rect(bounds);
        }

        var scaleX = bounds.Width / image.Width;
        var scaleY = bounds.Height / image.Height;
        switch (stretch)
        {
            case Stretch.Fill:
                return new Rect(bounds);
            case Stretch.UniformToFill:
                var fill = Math.Max(scaleX, scaleY);
                return Center(image * fill, bounds);
            case Stretch.None:
                return Center(image, bounds);
            default:
                var fit = Math.Min(scaleX, scaleY);
                return Center(image * fit, bounds);
        }

        static Rect Center(Size size, Size bounds)
            => new((bounds.Width - size.Width) / 2, (bounds.Height - size.Height) / 2, size.Width, size.Height);
    }

    private void OnFrameAvailable(object? sender, EventArgs e)
    {
        // Coalesce frames, the UI only needs the newest one.
        if (Interlocked.Exchange(ref _updatePending, 1) == 0)
        {
            Dispatcher.UIThread.Post(UpdateFrame, DispatcherPriority.Render);
        }
    }

    private void UpdateFrame()
    {
        Interlocked.Exchange(ref _updatePending, 0);

        var width = _frames.Width;
        var height = _frames.Height;
        if (width <= 0 || height <= 0 || _frames.Sequence == _lastSequence)
        {
            return;
        }

        if (_bitmap is null || _bitmap.PixelSize.Width != width || _bitmap.PixelSize.Height != height)
        {
            _bitmap?.Dispose();
            _bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        }

        using (var buffer = _bitmap.Lock())
        {
            if (!_frames.CopyTo(buffer.Address, buffer.RowBytes, width, height))
            {
                return;
            }
        }

        _lastSequence = _frames.Sequence;
        InvalidateVisual();
    }
}
