using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Views.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// Select user view.
/// </summary>
public partial class SelectUserView : BaseUserView<SelectUserViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserView"/> class.
    /// </summary>
    public SelectUserView()
        => AvaloniaXamlLoader.Load(this);
}
