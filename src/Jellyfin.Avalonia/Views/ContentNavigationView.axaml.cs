using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.ViewModels;
using Jellyfin.Avalonia.Views.Facades;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// Main content navigation view.
/// </summary>
public partial class ContentNavigationView : BaseUserView<ContentNavigationViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentNavigationView"/> class.
    /// </summary>
    public ContentNavigationView()
        => AvaloniaXamlLoader.Load(this);
}
