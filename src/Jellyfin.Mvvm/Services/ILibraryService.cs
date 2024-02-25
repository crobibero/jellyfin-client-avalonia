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
    /// Gets the library by id.
    /// </summary>
    /// <param name="id">The library id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The library.</returns>
    ValueTask<BaseItemDto?> GetLibraryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The item.</returns>
    ValueTask<BaseItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the library items.
    /// </summary>
    /// <param name="library">The library dto.</param>
    /// <param name="limit">The count of items to return.</param>
    /// <param name="startIndex">The first item index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The library items.</returns>
    ValueTask<BaseItemDtoQueryResult?> GetLibraryItemsAsync(BaseItemDto library, int limit, int startIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next up items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next up items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetNextUpAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the continue watching items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The continue watching items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetContinueWatchingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the recently added items.
    /// </summary>
    /// <param name="libraryId">The library id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The recently added library items.</returns>
    ValueTask<IReadOnlyList<BaseItemDto>> GetRecentlyAddedAsync(Guid libraryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the seasons in a series.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of seasons.</returns>
    ValueTask<BaseItemDtoQueryResult?> GetSeasonsAsync(Guid seriesId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next up item in a series.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next episode.</returns>
    ValueTask<BaseItemDtoQueryResult?> GetNextUpAsync(Guid seriesId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the episodes in a season.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <param name="seasonId">The season id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of episodes.</returns>
    ValueTask<BaseItemDtoQueryResult?> GetEpisodesAsync(Guid seriesId, Guid seasonId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the display preferences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The display preferences.</returns>
    ValueTask<DisplayPreferencesModel?> GetDisplayPreferencesAsync(CancellationToken cancellationToken = default);
}
