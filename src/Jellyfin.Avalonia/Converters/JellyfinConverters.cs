using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Jellyfin.Mvvm.Imaging;
using Jellyfin.Mvvm.Models;
using Material.Icons;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Value converters used by the views.
/// </summary>
public static class JellyfinConverters
{
    /// <summary>
    /// The card image height for the poster shape.
    /// </summary>
    public const double PosterHeight = 225;

    /// <summary>
    /// The card image height for the thumb shape.
    /// </summary>
    public const double ThumbHeight = 150;

    /// <summary>
    /// The card image height for the square shape.
    /// </summary>
    public const double SquareHeight = 180;

    // Blurhashes only hold a few low frequency components, so a small bitmap scales up without visible loss.
    private const int BlurHashSize = 32;

    // Recycled cards rebind constantly while scrolling, so decoded placeholders are reused by hash.
    private const int BlurHashCacheSize = 512;

    private static readonly Dictionary<string, LinkedListNode<(string Hash, ImageBrush Brush)>> _blurHashCache = new(StringComparer.Ordinal);
    private static readonly LinkedList<(string Hash, ImageBrush Brush)> _blurHashLru = new();

    /// <summary>
    /// Gets the converter from <see cref="CardShape"/> to the card width.
    /// </summary>
    public static IValueConverter CardWidth { get; } = new FuncValueConverter<CardShape, double>(GetCardWidth);

    /// <summary>
    /// Gets the converter from <see cref="CardShape"/> to the card image height.
    /// </summary>
    public static IValueConverter CardHeight { get; } = new FuncValueConverter<CardShape, double>(GetCardHeight);

    /// <summary>
    /// Gets the converter from <see cref="InfoChipStyle"/> to the chip background.
    /// </summary>
    public static IValueConverter ChipBackground { get; } = new FuncValueConverter<InfoChipStyle, IBrush>(style => style switch
    {
        InfoChipStyle.Default => ThemeBrush("ChipDefaultBrush"),
        InfoChipStyle.Green => ThemeBrush("ChipGreenBrush"),
        InfoChipStyle.Red => ThemeBrush("ChipRedBrush"),
        _ => Brushes.Transparent
    });

    /// <summary>
    /// Gets the converter from <see cref="InfoChipStyle"/> to the chip foreground.
    /// </summary>
    public static IValueConverter ChipForeground { get; } = new FuncValueConverter<InfoChipStyle, IBrush>(style => style switch
    {
        InfoChipStyle.Default => ThemeBrush("OnChipDefaultBrush"),
        _ => ThemeBrush("OnImageBrush")
    });

    /// <summary>
    /// Gets the converter from <see cref="InfoChipStyle"/> to the chip padding.
    /// </summary>
    public static IValueConverter ChipPadding { get; } = new FuncValueConverter<InfoChipStyle, Thickness>(style => style == InfoChipStyle.Transparent ? new Thickness(0) : new Thickness(8, 2));

    /// <summary>
    /// Gets the converter from <see cref="InfoChipIcon"/> to an icon.
    /// </summary>
    public static IValueConverter ChipIcon { get; } = new FuncValueConverter<InfoChipIcon, MaterialIconKind>(icon => icon switch
    {
        InfoChipIcon.Star => MaterialIconKind.Star,
        InfoChipIcon.CriticFresh => MaterialIconKind.FruitCherries,
        InfoChipIcon.CriticRotten => MaterialIconKind.Biohazard,
        InfoChipIcon.Time => MaterialIconKind.ClockOutline,
        _ => MaterialIconKind.Circle
    });

    /// <summary>
    /// Gets the converter from <see cref="InfoChipIcon"/> to the icon color.
    /// </summary>
    public static IValueConverter ChipIconBrush { get; } = new FuncValueConverter<InfoChipIcon, IBrush>(icon => icon switch
    {
        InfoChipIcon.Star => ThemeBrush("RatingStarBrush"),
        InfoChipIcon.CriticFresh => ThemeBrush("CriticFreshBrush"),
        InfoChipIcon.CriticRotten => ThemeBrush("CriticRottenBrush"),
        _ => ThemeBrush("OnImageBrush")
    });

    /// <summary>
    /// Gets the converter returning whether a chip has an icon.
    /// </summary>
    public static IValueConverter HasChipIcon { get; } = new FuncValueConverter<InfoChipIcon, bool>(icon => icon != InfoChipIcon.None);

    /// <summary>
    /// Gets the converter comparing a value to the converter parameter.
    /// </summary>
    public static IValueConverter EnumEquals { get; } = new EnumEqualsConverter();

    /// <summary>
    /// Gets the converter returning the play/pause icon.
    /// </summary>
    public static IValueConverter PlayPauseIcon { get; } = new FuncValueConverter<bool, MaterialIconKind>(isPlaying => isPlaying ? MaterialIconKind.Pause : MaterialIconKind.Play);

    /// <summary>
    /// Gets the converter returning the favorite icon.
    /// </summary>
    public static IValueConverter FavoriteIcon { get; } = new FuncValueConverter<bool, MaterialIconKind>(isFavorite => isFavorite ? MaterialIconKind.Heart : MaterialIconKind.HeartOutline);

    /// <summary>
    /// Gets the converter returning the watched icon.
    /// </summary>
    public static IValueConverter WatchedIcon { get; } = new FuncValueConverter<bool, MaterialIconKind>(isPlayed => isPlayed ? MaterialIconKind.CheckCircle : MaterialIconKind.CheckCircleOutline);

    /// <summary>
    /// Gets the converter returning the selection check opacity.
    /// </summary>
    public static IValueConverter SelectedOpacity { get; } = new FuncValueConverter<bool, double>(selected => selected ? 1 : 0);

    /// <summary>
    /// Gets the converter returning an error border thickness.
    /// </summary>
    public static IValueConverter ErrorBorder { get; } = new FuncValueConverter<bool, Thickness>(isError => isError ? new Thickness(1) : new Thickness(0));

    /// <summary>
    /// Gets the converter from ticks to a runtime.
    /// </summary>
    public static IValueConverter Runtime { get; } = new FuncValueConverter<long?, string?>(Formatting.FormatRuntime);

    /// <summary>
    /// Gets the converter from <see cref="CardShape"/> to the details poster width.
    /// </summary>
    public static IValueConverter DetailsPosterWidth { get; } = new FuncValueConverter<CardShape, double>(shape => shape switch
    {
        CardShape.Thumb => 420,
        CardShape.Square => 300,
        _ => 260
    });

    /// <summary>
    /// Gets the converter from <see cref="CardShape"/> to the details poster height.
    /// </summary>
    public static IValueConverter DetailsPosterHeight { get; } = new FuncValueConverter<CardShape, double>(shape => shape switch
    {
        CardShape.Thumb => 236,
        CardShape.Square => 300,
        _ => 390
    });

    /// <summary>
    /// Gets the converter from a blurhash to a placeholder brush.
    /// </summary>
    /// <remarks>
    /// A brush is used rather than an image source because changing it never invalidates measure.
    /// Item repeaters rebind recycled cards during layout, and an image source change there causes a layout loop.
    /// </remarks>
    public static IValueConverter BlurHashBrush { get; } = new FuncValueConverter<string?, IBrush?>(GetBlurHashBrush);

    /// <summary>
    /// Gets the converter returning the first character of a string.
    /// </summary>
    public static IValueConverter Initial { get; } = new FuncValueConverter<string?, string>(text => string.IsNullOrEmpty(text) ? string.Empty : text[..1].ToUpper(CultureInfo.CurrentCulture));

    /// <summary>
    /// Gets the card width for a shape.
    /// </summary>
    /// <param name="shape">The card shape.</param>
    /// <returns>The width.</returns>
    public static double GetCardWidth(CardShape shape) => shape switch
    {
        CardShape.Thumb => ThumbHeight * 16 / 9,
        CardShape.Square => SquareHeight,
        _ => PosterHeight * 2 / 3
    };

    /// <summary>
    /// Gets the card image height for a shape.
    /// </summary>
    /// <param name="shape">The card shape.</param>
    /// <returns>The height.</returns>
    public static double GetCardHeight(CardShape shape) => shape switch
    {
        CardShape.Thumb => ThumbHeight,
        CardShape.Square => SquareHeight,
        _ => PosterHeight
    };

    private static IBrush ThemeBrush(string key)
    {
        var app = Application.Current;
        return app is not null && app.TryGetResource(key, app.ActualThemeVariant, out var value) && value is IBrush brush
            ? brush
            : Brushes.Transparent;
    }

    private static ImageBrush? GetBlurHashBrush(string? blurHash)
    {
        if (string.IsNullOrEmpty(blurHash))
        {
            return null;
        }

        // Converters run on the UI thread, so the cache needs no locking.
        if (_blurHashCache.TryGetValue(blurHash, out var node))
        {
            _blurHashLru.Remove(node);
            _blurHashLru.AddFirst(node);
            return node.Value.Brush;
        }

        var brush = CreateBlurHashBrush(blurHash);
        if (brush is null)
        {
            return null;
        }

        _blurHashCache[blurHash] = _blurHashLru.AddFirst((blurHash, brush));
        if (_blurHashLru.Count > BlurHashCacheSize)
        {
            // Evicted brushes may still be shown by a card, so they are left to the garbage collector.
            _blurHashCache.Remove(_blurHashLru.Last!.Value.Hash);
            _blurHashLru.RemoveLast();
        }

        return brush;
    }

    private static ImageBrush? CreateBlurHashBrush(string blurHash)
    {
        const int RowLength = BlurHashSize * 4;
        var pixels = new byte[BlurHashSize * RowLength];
        if (!BlurHashDecoder.TryDecode(blurHash, BlurHashSize, BlurHashSize, pixels))
        {
            return null;
        }

        var bitmap = new WriteableBitmap(new PixelSize(BlurHashSize, BlurHashSize), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using var buffer = bitmap.Lock();
        for (var row = 0; row < BlurHashSize; row++)
        {
            Marshal.Copy(pixels, row * RowLength, buffer.Address + (row * buffer.RowBytes), RowLength);
        }

        return new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
    }

    private sealed class EnumEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is not null && parameter is not null
               && string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
