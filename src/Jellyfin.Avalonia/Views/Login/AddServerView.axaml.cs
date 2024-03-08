using Jellyfin.Avalonia.Views.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// Add server view.
/// </summary>
public partial class AddServerView : BaseUserView<AddServerViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerView"/> class.
    /// </summary>
    public AddServerView()
        => InitializeComponent();
}
