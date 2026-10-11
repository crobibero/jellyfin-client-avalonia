namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A query for library items.
/// </summary>
public sealed record ItemQuery
{
    /// <summary>
    /// Gets the parent id.
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// Gets the item types to include.
    /// </summary>
    public IReadOnlyList<BaseItemKind>? IncludeItemTypes { get; init; }

    /// <summary>
    /// Gets a value indicating whether to search recursively.
    /// </summary>
    public bool Recursive { get; init; } = true;

    /// <summary>
    /// Gets the sort fields.
    /// </summary>
    public IReadOnlyList<ItemSortBy> SortBy { get; init; } = [ItemSortBy.SortName];

    /// <summary>
    /// Gets the sort order.
    /// </summary>
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;

    /// <summary>
    /// Gets the played filter.
    /// </summary>
    public bool? IsPlayed { get; init; }

    /// <summary>
    /// Gets the favorite filter.
    /// </summary>
    public bool? IsFavorite { get; init; }

    /// <summary>
    /// Gets the genre ids to filter by.
    /// </summary>
    public IReadOnlyList<Guid>? GenreIds { get; init; }

    /// <summary>
    /// Gets the person ids to filter by.
    /// </summary>
    public IReadOnlyList<Guid>? PersonIds { get; init; }

    /// <summary>
    /// Gets the artist ids to filter by.
    /// </summary>
    public IReadOnlyList<Guid>? ArtistIds { get; init; }

    /// <summary>
    /// Gets the item ids to filter by.
    /// </summary>
    public IReadOnlyList<Guid>? Ids { get; init; }

    /// <summary>
    /// Gets the search term.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Gets the index of the first item.
    /// </summary>
    public int? StartIndex { get; init; }

    /// <summary>
    /// Gets the maximum number of items.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Gets the additional fields to return.
    /// </summary>
    public IReadOnlyList<ItemFields>? Fields { get; init; }

    /// <summary>
    /// Gets the media types to filter by.
    /// </summary>
    public IReadOnlyList<MediaType>? MediaTypes { get; init; }
}
