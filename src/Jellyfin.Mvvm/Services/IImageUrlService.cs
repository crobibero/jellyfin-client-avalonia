using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Builds image urls for items.
/// </summary>
public interface IImageUrlService
{
    /// <summary>
    /// Gets the card image for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="shape">The card shape.</param>
    /// <param name="maxHeight">The maximum height in pixels.</param>
    /// <param name="preferSeriesImages">Whether to prefer series artwork for episodes.</param>
    /// <returns>The image, or <c>null</c> when the item has no suitable image.</returns>
    ItemImage? GetCardImage(BaseItemDto item, CardShape shape, int maxHeight, bool preferSeriesImages = true);

    /// <summary>
    /// Gets the backdrop image url for an item, falling back to parent backdrops.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="maxWidth">The maximum width in pixels. Backdrops are always shown blurred, so the default is below screen width.</param>
    /// <returns>The image url, or <c>null</c>.</returns>
    string? GetBackdropUrl(BaseItemDto? item, int maxWidth = 1280);

    /// <summary>
    /// Gets the logo image url for an item, falling back to the parent logo.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="maxHeight">The maximum height in pixels.</param>
    /// <returns>The image url, or <c>null</c>.</returns>
    string? GetLogoUrl(BaseItemDto? item, int maxHeight = 200);

    /// <summary>
    /// Gets the primary image for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="maxHeight">The maximum height in pixels.</param>
    /// <returns>The image, or <c>null</c>.</returns>
    ItemImage? GetPrimaryImage(BaseItemDto? item, int maxHeight);

    /// <summary>
    /// Gets the image for a person.
    /// </summary>
    /// <param name="person">The person.</param>
    /// <param name="maxHeight">The maximum height in pixels.</param>
    /// <returns>The image, or <c>null</c>.</returns>
    ItemImage? GetPersonImage(BaseItemPerson person, int maxHeight);

    /// <summary>
    /// Gets the image url for a user.
    /// </summary>
    /// <remarks>
    /// The server ignores size options for user images and sends the original, so the client scales them down.
    /// </remarks>
    /// <param name="userId">The user id.</param>
    /// <param name="imageTag">The user's primary image tag.</param>
    /// <param name="serverUrl">The server url, defaults to the current server.</param>
    /// <returns>The image url, or <c>null</c>.</returns>
    string? GetUserImageUrl(Guid userId, string? imageTag, string? serverUrl = null);

    /// <summary>
    /// Gets the image url for a chapter.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="chapterIndex">The chapter index.</param>
    /// <param name="imageTag">The chapter image tag.</param>
    /// <param name="maxWidth">The maximum width in pixels.</param>
    /// <returns>The image url, or <c>null</c>.</returns>
    string? GetChapterImageUrl(Guid itemId, int chapterIndex, string? imageTag, int maxWidth);

    /// <summary>
    /// Gets the url of an image.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="imageType">The image type.</param>
    /// <param name="tag">The image tag.</param>
    /// <param name="maxHeight">The maximum height in pixels.</param>
    /// <param name="maxWidth">The maximum width in pixels.</param>
    /// <param name="index">The image index.</param>
    /// <returns>The image url, or <c>null</c> when no server is set or the image has no tag.</returns>
    string? GetImageUrl(Guid itemId, ImageType imageType, string? tag, int? maxHeight = null, int? maxWidth = null, int? index = null);
}
