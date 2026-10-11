using Avalonia.Controls;
using Jellyfin.Avalonia.Controls;
using Jellyfin.Mvvm.ViewModels;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The view for <see cref="LibraryViewModel"/>.
/// </summary>
public partial class LibraryView : PageView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryView"/> class.
    /// </summary>
    public LibraryView()
    {
        InitializeComponent();
        GridScroll.ScrollChanged += OnScrollChanged;
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        // Load the next page when the user gets close to the end.
        var remaining = GridScroll.Extent.Height - GridScroll.Viewport.Height - GridScroll.Offset.Y;
        if (remaining < GridScroll.Viewport.Height && DataContext is LibraryViewModel { HasMoreItems: true } viewModel)
        {
            viewModel.LoadMoreCommand.Execute(null);
        }
    }
}
