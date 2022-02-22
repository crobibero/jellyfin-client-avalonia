namespace Jellyfin.Maui.Models;

/// <summary>
/// Recently added model.
/// </summary>
public class RecentlyAddedModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecentlyAddedModel"/> class.
    /// </summary>
    /// <param name="libraryId">The library id.</param>
    /// <param name="libraryName">The library name.</param>
    /// <param name="items">The list of items.</param>
    public RecentlyAddedModel(Guid libraryId, string libraryName, IReadOnlyList<BaseItemDto> items)
    {
        LibraryId = libraryId;
        LibraryName = libraryName;
        Items = items;
    }

    /// <summary>
    /// Gets the library id.
    /// </summary>
    public Guid LibraryId { get; }

    /// <summary>
    /// Gets the library name.
    /// </summary>
    public string LibraryName { get; }

    /// <summary>
    /// Gets the list of recently added items.
    /// </summary>
    public IReadOnlyList<BaseItemDto> Items { get; }
}
