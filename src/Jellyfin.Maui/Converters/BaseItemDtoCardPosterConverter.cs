using System.Globalization;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// Gets the poster url from the base item.
/// </summary>
public class BaseItemDtoCardPosterConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(ImageSource) || value is not BaseItemDto baseItemDto)
        {
            return null;
        }

        var host = ServiceProvider.GetService<IStateService>().GetHost();

        var itemId = baseItemDto.Id;
        if(baseItemDto.Type == BaseItemKind.Episode)
        {
            itemId = baseItemDto.SeriesId ?? baseItemDto.Id;
        }

        return ImageSource.FromUri(new Uri($"{host}/Items/{itemId}/Images/{ImageType.Primary}"));
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
