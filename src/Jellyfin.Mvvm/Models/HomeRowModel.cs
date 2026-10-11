using CommunityToolkit.Mvvm.ComponentModel;

namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A row of cards.
/// </summary>
public partial class HomeRowModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    private IReadOnlyList<ItemCardModel> _items = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeRowModel"/> class.
    /// </summary>
    /// <param name="name">The row name.</param>
    /// <param name="shape">The card shape.</param>
    public HomeRowModel(string name, CardShape shape = CardShape.Poster)
    {
        Name = name;
        Shape = shape;
    }

    /// <summary>
    /// Gets the row name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the card shape.
    /// </summary>
    public CardShape Shape { get; }

    /// <summary>
    /// Gets a value indicating whether the row has any items.
    /// </summary>
    public bool HasItems => Items.Count > 0;
}
