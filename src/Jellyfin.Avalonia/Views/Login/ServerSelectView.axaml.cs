using Jellyfin.Avalonia.Controls;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// The view for <see cref="Jellyfin.Mvvm.ViewModels.Login.ServerSelectViewModel"/>.
/// </summary>
public partial class ServerSelectView : PageView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectView"/> class.
    /// </summary>
    public ServerSelectView()
        => InitializeComponent();
}
