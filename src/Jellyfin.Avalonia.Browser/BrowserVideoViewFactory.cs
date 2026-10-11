using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// The video element sits behind the canvas, so the view is an empty transparent control.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class BrowserVideoViewFactory : IVideoViewFactory
{
    /// <inheritdoc />
    public bool RendersBehindContent => true;

    /// <inheritdoc />
    public Control? CreateVideoView(IMediaPlayer player) => new BrowserVideoView();

    private sealed class BrowserVideoView : Control, IVideoView
    {
        // A single transparent black pixel, drawn with source blending to punch through to the video element.
        private static readonly Lazy<WriteableBitmap> _clearPixel = new(CreateClearPixel);

        private Stretch _stretch = Stretch.Uniform;

        public override void Render(DrawingContext context)
        {
            // The browser canvas is cleared with Colors.Transparent (#00FFFFFF), which the page composites as white.
            // Overwrite the area with transparent black so the video element behind the canvas shows through.
            using (context.PushRenderOptions(new RenderOptions { BitmapBlendingMode = BitmapBlendingMode.Source }))
            {
                context.DrawImage(_clearPixel.Value, new Rect(0, 0, 1, 1), new Rect(Bounds.Size));
            }
        }

        private static WriteableBitmap CreateClearPixel()
        {
            var bitmap = new WriteableBitmap(new PixelSize(1, 1), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
            using var buffer = bitmap.Lock();
            System.Runtime.InteropServices.Marshal.WriteInt32(buffer.Address, 0);
            return bitmap;
        }

        public Stretch VideoStretch
        {
            get => _stretch;
            set
            {
                _stretch = value;
                BrowserInterop.SetObjectFit(value switch
                {
                    Stretch.UniformToFill => "cover",
                    Stretch.Fill => "fill",
                    _ => "contain"
                });
            }
        }
    }
}
