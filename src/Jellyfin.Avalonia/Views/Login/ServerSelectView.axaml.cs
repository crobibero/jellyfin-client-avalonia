using Jellyfin.Avalonia.Views.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// Server select view.
/// </summary>
public partial class ServerSelectView : BaseUserView<ServerSelectViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectView"/> class.
    /// </summary>
    public ServerSelectView()
        => InitializeComponent();
}
