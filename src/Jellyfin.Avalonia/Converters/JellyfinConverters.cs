using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentUIIconsLike;
using Jellyfin.Mvvm.Models;

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
    public static IValueConverter ChipIcon { get; } = new FuncValueConverter<InfoChipIcon, FluentIconSymbol>(icon => icon switch
    {
        InfoChipIcon.Star => FluentIconSymbol.Star,
        InfoChipIcon.CriticFresh => FluentIconSymbol.ThumbLike,
        InfoChipIcon.CriticRotten => FluentIconSymbol.ThumbDislike,
        InfoChipIcon.Time => FluentIconSymbol.Clock,
        _ => FluentIconSymbol.Circle
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
    public static IValueConverter PlayPauseIcon { get; } = new FuncValueConverter<bool, FluentIconSymbol>(isPlaying => isPlaying ? FluentIconSymbol.Pause : FluentIconSymbol.Play);

    /// <summary>
    /// Gets the converter returning the filled icon style when true and the regular style otherwise.
    /// </summary>
    public static IValueConverter FilledIcon { get; } = new FuncValueConverter<bool, FluentIconStyle>(filled => filled ? FluentIconStyle.Filled : FluentIconStyle.Regular);

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

    private sealed class EnumEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is not null && parameter is not null
               && string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
