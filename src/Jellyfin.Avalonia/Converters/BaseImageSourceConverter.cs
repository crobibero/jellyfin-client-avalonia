using System.Globalization;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Generate the image url from the item.
/// </summary>
public class BaseImageSourceConverter : BaseConverterOneWay<BaseItemDto?, string?, int?>
{
    private readonly ImageType _imageType;
    private readonly IImageClient _imageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseImageSourceConverter"/> class.
    /// </summary>
    /// <param name="imageType">The image type to fetch.</param>
    protected BaseImageSourceConverter(ImageType imageType)
    {
        _imageClient = App.Current.ServiceProvider.GetRequiredService<IImageClient>();
        _imageType = imageType;
    }

    /// <inheritdoc />
    protected override string? ConvertFrom(BaseItemDto? value, int? parameter, CultureInfo? culture)
    {
        if (value is null)
        {
            return null;
        }

        var itemId = value.Id;

        var imageTypeStr = _imageType.ToString();

        /*
         * If the item is an Episode or Season and the requested image type doesn't exist,
         * request the Series' image.
         */
        if ((value.Type is BaseItemKind.Episode or BaseItemKind.Season)
            && !value.ImageTags.Keys.Contains(imageTypeStr, StringComparer.OrdinalIgnoreCase))
        {
            itemId = value.SeriesId ?? value.Id;
        }

        return _imageClient.GetItemImageUrl(itemId, _imageType, maxHeight: parameter);
    }
}
