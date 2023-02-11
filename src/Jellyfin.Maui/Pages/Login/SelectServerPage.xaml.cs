using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The select server page.
/// </summary>
public partial class SelectServerPage : BaseContentPage<ServerSelectViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectServerPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="ServerSelectViewModel"/>.</param>
    public SelectServerPage(ServerSelectViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
