using System.Globalization;
using System.Text;
using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class ImageUrlService : IImageUrlService
{
    private const int Quality = 90;
    private readonly JellyfinSdkSettings _sdkSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageUrlService"/> class.
    /// </summary>
    /// <param name="sdkSettings">The sdk settings.</param>
    public ImageUrlService(JellyfinSdkSettings sdkSettings)
    {
        _sdkSettings = sdkSettings;
    }

    /// <inheritdoc />
    public ItemImage? GetCardImage(BaseItemDto item, CardShape shape, int maxHeight, bool preferSeriesImages = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        var image = shape switch
        {
            CardShape.Thumb => SelectThumb(item, preferSeriesImages),
            CardShape.Square => SelectSquare(item),
            _ => SelectPoster(item)
        };

        return GetItemImage(image, item.ImageBlurHashes, maxHeight);
    }

    /// <inheritdoc />
    public string? GetBackdropUrl(BaseItemDto? item, int maxWidth = 1280)
    {
        if (item is null)
        {
            return null;
        }

        var image = Select(item.Id, ImageType.Backdrop, FirstTag(item.BackdropImageTags), 0)
            ?? Select(item.ParentBackdropItemId, ImageType.Backdrop, FirstTag(item.ParentBackdropImageTags), 0);
        return image is { } selected ? GetImageUrl(selected, maxWidth: maxWidth) : null;
    }

    /// <inheritdoc />
    public string? GetLogoUrl(BaseItemDto? item, int maxHeight = 200)
    {
        if (item is null)
        {
            return null;
        }

        var image = Select(item.Id, ImageType.Logo, GetTag(item, ImageType.Logo))
            ?? Select(item.ParentLogoItemId, ImageType.Logo, item.ParentLogoImageTag);
        return image is { } selected ? GetImageUrl(selected, maxHeight: maxHeight) : null;
    }

    /// <inheritdoc />
    public ItemImage? GetPrimaryImage(BaseItemDto? item, int maxHeight)
    {
        if (item is null)
        {
            return null;
        }

        return GetItemImage(Select(item.Id, ImageType.Primary, GetTag(item, ImageType.Primary)), item.ImageBlurHashes, maxHeight);
    }

    /// <inheritdoc />
    public ItemImage? GetPersonImage(BaseItemPerson person, int maxHeight)
    {
        ArgumentNullException.ThrowIfNull(person);

        if (Select(person.Id, ImageType.Primary, person.PrimaryImageTag) is not { } image
            || GetImageUrl(image, maxHeight: maxHeight) is not { } url)
        {
            return null;
        }

        return new ItemImage(url, GetBlurHash(person.ImageBlurHashes?.Primary?.AdditionalData, image.Tag));
    }

    /// <inheritdoc />
    public string? GetUserImageUrl(Guid userId, string? imageTag, string? serverUrl = null)
    {
        var baseUrl = (serverUrl ?? _sdkSettings.ServerUrl)?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(imageTag))
        {
            return null;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{baseUrl}/Users/{userId:N}/Images/Primary?tag={Uri.EscapeDataString(imageTag)}&format=Webp");
    }

    /// <inheritdoc />
    public string? GetChapterImageUrl(Guid itemId, int chapterIndex, string? imageTag, int maxWidth)
        => GetImageUrl(itemId, ImageType.Chapter, imageTag, maxWidth: maxWidth, index: chapterIndex);

    /// <inheritdoc />
    public string? GetImageUrl(Guid itemId, ImageType imageType, string? tag, int? maxHeight = null, int? maxWidth = null, int? index = null)
    {
        var baseUrl = _sdkSettings.ServerUrl?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl) || itemId == Guid.Empty || string.IsNullOrEmpty(tag))
        {
            return null;
        }

        var builder = new StringBuilder(baseUrl)
            .Append("/Items/")
            .Append(itemId.ToString("N", CultureInfo.InvariantCulture))
            .Append("/Images/")
            .Append(imageType.ToString());

        if (index is not null)
        {
            builder.Append('/').Append(index.Value.ToString(CultureInfo.InvariantCulture));
        }

        builder.Append("?quality=").Append(Quality.ToString(CultureInfo.InvariantCulture));

        if (maxHeight is not null)
        {
            builder.Append("&fillHeight=").Append(maxHeight.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (maxWidth is not null)
        {
            builder.Append("&fillWidth=").Append(maxWidth.Value.ToString(CultureInfo.InvariantCulture));
        }

        builder.Append("&tag=").Append(Uri.EscapeDataString(tag));

        return builder.ToString();
    }

    internal static string? GetTag(BaseItemDto item, ImageType imageType)
    {
        var tags = item.ImageTags?.AdditionalData;
        if (tags is null)
        {
            return null;
        }

        var key = imageType.ToString();
        foreach (var (tagKey, value) in tags)
        {
            if (string.Equals(tagKey, key, StringComparison.OrdinalIgnoreCase))
            {
                return value?.ToString();
            }
        }

        return null;
    }

    // The server also adds the blurhashes of inherited images, such as the series poster, to the item.
    internal static string? GetBlurHash(BaseItemDto_ImageBlurHashes? blurHashes, ImageType imageType, string tag)
    {
        var hashes = imageType switch
        {
            ImageType.Primary => blurHashes?.Primary?.AdditionalData,
            ImageType.Thumb => blurHashes?.Thumb?.AdditionalData,
            ImageType.Backdrop => blurHashes?.Backdrop?.AdditionalData,
            _ => null
        };

        return GetBlurHash(hashes, tag);
    }

    private static string? GetBlurHash(IDictionary<string, object>? hashes, string tag)
        => hashes is not null && hashes.TryGetValue(tag, out var hash) && hash?.ToString() is { Length: > 0 } value ? value : null;

    private static string? FirstTag(List<string>? tags) => tags is { Count: > 0 } ? tags[0] : null;

    private static ImageRef? Select(Guid? itemId, ImageType imageType, string? tag, int? index = null)
        => itemId is { } id && id != Guid.Empty && !string.IsNullOrEmpty(tag) ? new ImageRef(id, imageType, tag, index) : null;

    private static ImageRef? SelectPoster(BaseItemDto item)
    {
        // Episodes use the series poster when shown as posters.
        if (item.Type is BaseItemDto_Type.Episode && Select(item.SeriesId, ImageType.Primary, item.SeriesPrimaryImageTag) is { } series)
        {
            return series;
        }

        return Select(item.Id, ImageType.Primary, GetTag(item, ImageType.Primary))
            ?? (item.Type is BaseItemDto_Type.Season ? Select(item.SeriesId, ImageType.Primary, item.SeriesPrimaryImageTag) : null)
            ?? Select(item.AlbumId, ImageType.Primary, item.AlbumPrimaryImageTag);
    }

    private static ImageRef? SelectThumb(BaseItemDto item, bool preferSeriesImages)
    {
        if (item.Type is BaseItemDto_Type.Episode)
        {
            var seriesImage = preferSeriesImages
                ? Select(item.SeriesId, ImageType.Thumb, item.SeriesThumbImageTag)
                    ?? Select(item.ParentThumbItemId, ImageType.Thumb, item.ParentThumbImageTag)
                : null;

            // Episode primary images are screenshots.
            if ((seriesImage ?? Select(item.Id, ImageType.Primary, GetTag(item, ImageType.Primary))) is { } episodeImage)
            {
                return episodeImage;
            }
        }

        var primaryIsThumb = item.Type is BaseItemDto_Type.CollectionFolder or BaseItemDto_Type.UserView or BaseItemDto_Type.Folder
            or BaseItemDto_Type.Video or BaseItemDto_Type.MusicVideo or BaseItemDto_Type.TvChannel or BaseItemDto_Type.Program;

        return Select(item.Id, ImageType.Thumb, GetTag(item, ImageType.Thumb))
            ?? (primaryIsThumb ? Select(item.Id, ImageType.Primary, GetTag(item, ImageType.Primary)) : null)
            ?? Select(item.Id, ImageType.Backdrop, FirstTag(item.BackdropImageTags), 0)
            ?? Select(item.ParentThumbItemId, ImageType.Thumb, item.ParentThumbImageTag)
            ?? Select(item.ParentBackdropItemId, ImageType.Backdrop, FirstTag(item.ParentBackdropImageTags), 0);
    }

    private static ImageRef? SelectSquare(BaseItemDto item)
        => Select(item.Id, ImageType.Primary, GetTag(item, ImageType.Primary))
            ?? Select(item.AlbumId, ImageType.Primary, item.AlbumPrimaryImageTag);

    private string? GetImageUrl(ImageRef image, int? maxHeight = null, int? maxWidth = null)
        => GetImageUrl(image.ItemId, image.Type, image.Tag, maxHeight, maxWidth, image.Index);

    private ItemImage? GetItemImage(ImageRef? image, BaseItemDto_ImageBlurHashes? blurHashes, int maxHeight)
    {
        if (image is not { } selected || GetImageUrl(selected, maxHeight: maxHeight) is not { } url)
        {
            return null;
        }

        return new ItemImage(url, GetBlurHash(blurHashes, selected.Type, selected.Tag));
    }

    private readonly record struct ImageRef(Guid ItemId, ImageType Type, string Tag, int? Index);
}
