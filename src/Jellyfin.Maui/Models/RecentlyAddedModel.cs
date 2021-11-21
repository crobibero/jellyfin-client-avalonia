using Jellyfin.Sdk;

namespace Jellyfin.Maui.Models;

/// <summary>
/// Recently added model.
/// </summary>
public class RecentlyAddedModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecentlyAddedModel"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="items"></param>
    public RecentlyAddedModel(Guid id, string name, IReadOnlyList<BaseItemDto> items)
    {
        Id = id;
        Name = name;
        Items = items;
    }

    /// <summary>
    /// Gets the library id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the library name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the list of recently added items.
    /// </summary>
    public IReadOnlyList<BaseItemDto> Items { get; }
}
