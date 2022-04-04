using CommunityToolkit.Mvvm.ComponentModel;

namespace Jellyfin.Maui.Models;

/// <summary>
/// The home row model.
/// </summary>
[INotifyPropertyChanged]
public partial class HomeRowModel
{
    [ObservableProperty]
    private IReadOnlyList<BaseItemDto> _items = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeRowModel"/> class.
    /// </summary>
    /// <param name="name">The home row name.</param>
    public HomeRowModel(string name)
    {
        Name = name;
        Items = Array.Empty<BaseItemDto>();
    }

    /// <summary>
    /// Gets the home row name.
    /// </summary>
    public string Name { get; }
}
