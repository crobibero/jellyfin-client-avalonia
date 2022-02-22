using System.Globalization;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// BaseItemDto Card Title Converter.
/// </summary>
public class BaseItemDtoCardTitleConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not BaseItemDto baseItemDto)
        {
            return string.Empty;
        }

        return baseItemDto.Type switch
        {
            BaseItemKind.Episode => baseItemDto.SeriesName,
            BaseItemKind.Season => baseItemDto.SeriesName,
            _ => baseItemDto.Name?.ToString(culture) ?? string.Empty
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
