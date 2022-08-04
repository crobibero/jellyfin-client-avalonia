using System.Globalization;
using CommunityToolkit.Maui.Converters;
using Jellyfin.Maui.Extensions;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.Converters;

/// <summary>
/// Gets the poster url from the base item.
/// </summary>
public class BaseImageSourceConverter : BaseConverterOneWay<BaseItemDto?, ImageSource?>
{
    private readonly ImageType _imageType;
    private readonly IImageClient _imageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseImageSourceConverter"/> class.
    /// </summary>
    /// <param name="imageType">The image type to fetch.</param>
    public BaseImageSourceConverter(ImageType imageType)
    {
        _imageType = imageType;
        _imageClient = InternalServiceProvider.GetService<IImageClient>();
    }

    /// <inheritdoc/>
    public override ImageSource? ConvertFrom(BaseItemDto? value, CultureInfo? culture)
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

        return ImageSource.FromUri(_imageClient.GetItemImageUrl(itemId, _imageType).ToUri());
    }
}
