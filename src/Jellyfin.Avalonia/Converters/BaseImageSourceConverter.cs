using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Generate the image url from the item.
/// </summary>
public class BaseImageSourceConverter : IValueConverter
{
    private readonly ImageType _imageType;
    private readonly IImageClient _imageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseImageSourceConverter"/> class.
    /// </summary>
    /// <param name="imageType">The image type to fetch.</param>
    protected BaseImageSourceConverter(ImageType imageType)
    {
        var serviceProvider = (IServiceProvider)Application.Current!.Resources[typeof(IServiceProvider)]!;
        _imageClient = serviceProvider.GetRequiredService<IImageClient>();
        _imageType = imageType;
    }

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BaseItemDto item)
        {
            return null;
        }

        int? maxHeight = null;

        if (int.TryParse(parameter?.ToString(), out int tmp))
        {
            maxHeight = tmp;
        }

        var itemId = item.Id;

        var imageTypeStr = _imageType.ToString();

        /*
         * If the item is an Episode or Season and the requested image type doesn't exist,
         * request the Series' image.
         */
        if ((item.Type is BaseItemKind.Episode or BaseItemKind.Season)
            && !item.ImageTags.Keys.Contains(imageTypeStr, StringComparer.OrdinalIgnoreCase))
        {
            itemId = item.SeriesId ?? item.Id;
        }

        return _imageClient.GetItemImageUrl(itemId, _imageType, maxHeight: maxHeight);
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
