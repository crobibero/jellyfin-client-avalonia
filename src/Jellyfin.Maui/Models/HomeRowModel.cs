namespace Jellyfin.Maui.Models;

/// <summary>
/// The home row model.
/// </summary>
public class HomeRowModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeRowModel"/> class.
    /// </summary>
    /// <param name="name">The home row name.</param>
    /// <param name="order">The sort order.</param>
    /// <param name="items">The list of items.</param>
    public HomeRowModel(string name, int order, IReadOnlyList<BaseItemDto> items)
    {
        Name = name;
        Order = order;
        Items = items;
    }

    /// <summary>
    /// Gets the home row name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the sort order.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets or sets the list of items in the home row.
    /// </summary>
    public IReadOnlyList<BaseItemDto> Items { get; set; }
}
