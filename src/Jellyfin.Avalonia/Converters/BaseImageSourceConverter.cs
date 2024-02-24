using System.Globalization;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Models;

using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Generate the image url from the item.
/// </summary>
public class BaseImageSourceConverter : BaseConverterOneWay<BaseItemDto?, string?, int?>
{
    private readonly ImageType _imageType;
    private readonly JellyfinApiClient _jellyfinApiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseImageSourceConverter"/> class.
    /// </summary>
    /// <param name="imageType">The image type to fetch.</param>
    protected BaseImageSourceConverter(ImageType imageType)
    {
        _jellyfinApiClient = App.Current.ServiceProvider.GetRequiredService<JellyfinApiClient>();
        _imageType = imageType;
    }

    /// <inheritdoc />
    protected override string? ConvertFrom(BaseItemDto? value, int? parameter, CultureInfo? culture)
    {
        if (value?.Id is null)
        {
            return null;
        }

        var itemId = value.Id.Value;

        var imageTypeStr = _imageType.ToString();

        /*
         * If the item is an Episode or Season and the requested image type doesn't exist,
         * request the Series' image.
         */
        if ((value.Type is BaseItemDto_Type.Episode or BaseItemDto_Type.Season)
            && !value.ImageTags!.AdditionalData.Keys.Contains(imageTypeStr, StringComparer.OrdinalIgnoreCase))
        {
            itemId = value.SeriesId ?? itemId;
        }

        var requestInfo = _jellyfinApiClient.Items[itemId].Images[_imageType.ToString()].ToGetRequestInformation(c =>
        {
            c.QueryParameters.MaxHeight = parameter;
        });

        return requestInfo.URI.ToString();
    }
}
