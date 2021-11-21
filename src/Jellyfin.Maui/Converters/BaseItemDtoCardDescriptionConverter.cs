using System.Globalization;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// BaseItemDto Card Description Converter.
/// </summary>
public class BaseItemDtoCardDescriptionConverter : IValueConverter
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
            BaseItemKind.Episode => $"S{baseItemDto.ParentIndexNumber} E{baseItemDto.IndexNumber} {baseItemDto.Name}",
            BaseItemKind.Season => baseItemDto.SeasonName,
            _ => baseItemDto.ProductionYear?.ToString(culture) ?? string.Empty
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
