using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Views.Facades;
using Jellyfin.Mvvm.ViewModels;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The home view.
/// </summary>
public partial class HomeView : BaseUserView<HomeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeView"/> class.
    /// </summary>
    public HomeView()
        => AvaloniaXamlLoader.Load(this);
}

