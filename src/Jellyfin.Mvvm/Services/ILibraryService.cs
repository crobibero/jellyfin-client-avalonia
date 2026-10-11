using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Library service interface.
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// Gets the list of visible libraries.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of libraries.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetLibrariesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The item.</returns>
    ValueTask<BaseItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query library items.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query result.</returns>
    ValueTask<BaseItemDtoQueryResult> GetItemsAsync(ItemQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next up items.
    /// </summary>
    /// <param name="seriesId">The series id, or <c>null</c> for all series.</param>
    /// <param name="limit">The maximum number of items.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next up items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetNextUpAsync(Guid? seriesId = null, int limit = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the continue watching items.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The continue watching items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetResumeAsync(MediaType mediaType = MediaType.Video, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the recently added items.
    /// </summary>
    /// <param name="libraryId">The library id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The recently added library items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetLatestAsync(Guid libraryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the seasons in a series.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of seasons.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetSeasonsAsync(Guid seriesId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the episodes in a series.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="seasonId">The season id, or <c>null</c> for all seasons.</param>
    /// <param name="startItemId">The episode to start at.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of episodes.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetEpisodesAsync(Guid seriesId, Guid? seasonId = null, Guid? startItemId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets items similar to an item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The similar items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetSimilarAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the special features of an item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The special features.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetSpecialFeaturesAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the local trailers of an item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The trailers.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetLocalTrailersAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the media segments of an item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media segments.</returns>
    ValueTask<IReadOnlyList<MediaSegmentDto>> GetMediaSegmentsAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the favorite state of an item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="isFavorite">Whether the item is a favorite.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user data.</returns>
    ValueTask<UserItemDataDto?> SetFavoriteAsync(Guid itemId, bool isFavorite, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the played state of an item.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="isPlayed">Whether the item is played.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated user data.</returns>
    ValueTask<UserItemDataDto?> SetPlayedAsync(Guid itemId, bool isPlayed, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the display preferences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The display preferences.</returns>
    ValueTask<DisplayPreferencesModel?> GetDisplayPreferencesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search people.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="limit">The maximum number of results.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The people.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> SearchPeopleAsync(string searchTerm, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the items to play for an item, expanding folders such as series, seasons and albums.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The playable items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetPlayableItemsAsync(BaseItemDto item, CancellationToken cancellationToken = default);
}
