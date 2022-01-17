using System.Globalization;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// Gets the poster url from the base item.
/// </summary>
public class BaseItemDtoCardPosterConverter : IValueConverter
{
    private readonly ImageType _imageType;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseItemDtoCardPosterConverter"/> class.
    /// </summary>
    /// <param name="imageType">The image type to fetch.</param>
    public BaseItemDtoCardPosterConverter(ImageType imageType)
    {
        _imageType = imageType;
    }

    /// <inheritdoc />
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(ImageSource) || value is not BaseItemDto baseItemDto)
        {
            return null;
        }

        var host = InternalServiceProvider.GetService<IStateService>().GetHost();

        var itemId = baseItemDto.Id;
        if (baseItemDto.Type == BaseItemKind.Episode)
        {
            itemId = baseItemDto.SeriesId ?? baseItemDto.Id;
        }

        return ImageSource.FromUri(new Uri($"{host}/Items/{itemId}/Images/{_imageType}?maxHeight=480"));
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
