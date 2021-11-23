using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Library service interface.
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// Gets the list of visible libraries.
    /// </summary>
    /// <returns>The list of libraries.</returns>
    Task<IReadOnlyList<BaseItemDto>> GetLibraries();

    /// <summary>
    /// Gets the library by id.
    /// </summary>
    /// <param name="id">The library id.</param>
    /// <returns>The library.</returns>
    Task<BaseItemDto?> GetLibrary(Guid id);

    /// <summary>
    /// Gets the item by id.
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <returns>The item.</returns>
    Task<BaseItemDto?> GetItem(Guid id);

    /// <summary>
    /// Gets the library items.
    /// </summary>
    /// <param name="library">The library dto.</param>
    /// <param name="limit">The count of items to return.</param>
    /// <param name="startIndex">The first item index.</param>
    /// <returns>The library items.</returns>
    Task<BaseItemDtoQueryResult> GetLibraryItems(BaseItemDto library, int limit, int startIndex);

    /// <summary>
    /// Gets the next up items.
    /// </summary>
    /// <param name="libraryIds">The list of library ids.</param>
    /// <returns>The next up items.</returns>
    Task<IReadOnlyList<BaseItemDto>> GetNextUp(IEnumerable<Guid> libraryIds);

    /// <summary>
    /// Gets the continue watching items.
    /// </summary>
    /// <returns>The continue watching items.</returns>
    Task<IReadOnlyList<BaseItemDto>> GetContinueWatching();

    /// <summary>
    /// Gets the recently added items.
    /// </summary>
    /// <param name="libraryId">The library id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The recently added library items.</returns>
    Task<IReadOnlyList<BaseItemDto>> GetRecentlyAdded(Guid libraryId, CancellationToken cancellationToken = default);
}
