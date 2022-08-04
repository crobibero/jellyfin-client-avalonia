using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The select user page.
/// </summary>
public partial class SelectUserPage : BaseContentPage<SelectUserViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SelectUserViewModel"/>.</param>
    public SelectUserPage(SelectUserViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
