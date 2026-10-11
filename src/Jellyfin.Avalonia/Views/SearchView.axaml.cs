using Jellyfin.Avalonia.Controls;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The view for <see cref="Jellyfin.Mvvm.ViewModels.SearchViewModel"/>.
/// </summary>
public partial class SearchView : PageView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchView"/> class.
    /// </summary>
    public SearchView()
        => InitializeComponent();
}
