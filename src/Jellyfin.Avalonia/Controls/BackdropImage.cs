using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Jellyfin.Mvvm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Controls;

/// <summary>
/// Shows a blurred, dimmed backdrop and cross fades between images.
/// </summary>
/// <remarks>
/// Backdrops bypass the shared image loader cache, which is bounded by item count while a decoded backdrop is far
/// larger than a poster. Here they are decoded to a capped width and only the last few are kept.
/// </remarks>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Controls are not disposable; the pending load is cancelled and disposed when the control is detached.")]
public class BackdropImage : Panel
{
    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<BackdropImage, string?>(nameof(Source));

    // The image is blurred, so decoding wider than this only costs memory.
    private const int DecodeWidth = 1280;
    private const int CacheSize = 4;

    private static readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(400);

    // Shared by every backdrop control and only touched on the UI thread.
    private static readonly LinkedList<(string Url, Bitmap Bitmap)> _cache = new();

    // How many images show each bitmap. A bitmap is disposed once it is neither cached nor shown.
    private static readonly Dictionary<Bitmap, int> _shown = new(ReferenceEqualityComparer.Instance);

    private static HttpClient? _httpClient;

    private readonly Image _front;
    private readonly Image _back;
    private IDisposable? _swapTimer;
    private CancellationTokenSource? _loadCancellation;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackdropImage"/> class.
    /// </summary>
    public BackdropImage()
    {
        _front = CreateImage();
        _back = CreateImage();
        Children.Add(_back);
        Children.Add(_front);
        IsHitTestVisible = false;
    }

    /// <summary>
    /// Gets or sets the image url.
    /// </summary>
    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private static HttpClient HttpClient
        => _httpClient ??= App.Services.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientNames.Default);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceProperty)
        {
            // Delay slightly so quickly moving focus across cards doesn't load every backdrop.
            _swapTimer?.Dispose();
            _swapTimer = DispatcherTimer.RunOnce(() => LoadAsync(Source).SafeFireAndForget(), TimeSpan.FromMilliseconds(250));
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _swapTimer?.Dispose();
        _loadCancellation?.Cancel();
        _loadCancellation?.Dispose();
        _loadCancellation = null;
        base.OnDetachedFromVisualTree(e);
    }

    private static Image CreateImage()
        => new()
        {
            Stretch = Stretch.UniformToFill,
            Opacity = 0,
            Effect = new BlurEffect { Radius = 12 },
            Transitions =
            [
                new DoubleTransition { Property = OpacityProperty, Duration = _fadeDuration },
            ],
        };

    private static Bitmap? GetCached(string url)
    {
        for (var node = _cache.First; node is not null; node = node.Next)
        {
            if (string.Equals(node.Value.Url, url, StringComparison.Ordinal))
            {
                _cache.Remove(node);
                _cache.AddFirst(node);
                return node.Value.Bitmap;
            }
        }

        return null;
    }

    private static async Task<Bitmap> DecodeAsync(string url, CancellationToken cancellationToken)
    {
        var bytes = await HttpClient.GetByteArrayAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
        return await Task.Run(
                () =>
                {
                    using var stream = new MemoryStream(bytes, writable: false);
                    return Bitmap.DecodeToWidth(stream, DecodeWidth, BitmapInterpolationMode.MediumQuality);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task LoadAsync(string? url)
    {
        if (_loadCancellation is not null)
        {
            await _loadCancellation.CancelAsync().ConfigureAwait(true);
            _loadCancellation.Dispose();
            _loadCancellation = null;
        }

        if (string.IsNullOrEmpty(url))
        {
            Show(null);
            return;
        }

        var bitmap = GetCached(url);
        if (bitmap is null)
        {
            var cancellation = _loadCancellation = new CancellationTokenSource();
            try
            {
                bitmap = await DecodeAsync(url, cancellation.Token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (HttpRequestException)
            {
                Show(null);
                return;
            }

            if (cancellation.IsCancellationRequested || !string.Equals(url, Source, StringComparison.Ordinal))
            {
                bitmap.Dispose();
                return;
            }

            AddToCache(url, bitmap);
        }

        Show(bitmap);
    }

    private static void AddToCache(string url, Bitmap bitmap)
    {
        _cache.AddFirst((url, bitmap));
        while (_cache.Count > CacheSize)
        {
            var evicted = _cache.Last!.Value.Bitmap;
            _cache.RemoveLast();
            if (!_shown.ContainsKey(evicted))
            {
                evicted.Dispose();
            }
        }
    }

    private static bool IsCached(Bitmap bitmap)
    {
        foreach (var entry in _cache)
        {
            if (ReferenceEquals(entry.Bitmap, bitmap))
            {
                return true;
            }
        }

        return false;
    }

    private static void SetSource(Image image, Bitmap? bitmap)
    {
        var previous = image.Source as Bitmap;
        if (ReferenceEquals(previous, bitmap))
        {
            return;
        }

        image.Source = bitmap;
        if (bitmap is not null)
        {
            _shown[bitmap] = _shown.GetValueOrDefault(bitmap) + 1;
        }

        if (previous is null)
        {
            return;
        }

        if (_shown[previous] > 1)
        {
            _shown[previous]--;
            return;
        }

        _shown.Remove(previous);
        if (!IsCached(previous))
        {
            previous.Dispose();
        }
    }

    private void Show(Bitmap? bitmap)
    {
        // The hidden image takes the new bitmap and fades in over the visible one.
        var incoming = _front.Opacity > 0 ? _back : _front;
        var outgoing = ReferenceEquals(incoming, _front) ? _back : _front;

        SetSource(incoming, bitmap);
        incoming.Opacity = bitmap is null ? 0 : 1;
        outgoing.Opacity = 0;

        incoming.ZIndex = 1;
        outgoing.ZIndex = 0;
    }
}
