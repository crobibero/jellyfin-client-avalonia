using Jellyfin.Avalonia.Controls;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The view for <see cref="Jellyfin.Mvvm.ViewModels.HomeViewModel"/>.
/// </summary>
public partial class HomeView : PageView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeView"/> class.
    /// </summary>
    public HomeView()
        => InitializeComponent();
}
