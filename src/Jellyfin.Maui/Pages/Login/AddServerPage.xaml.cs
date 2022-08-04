using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The add server page.
/// </summary>
public partial class AddServerPage : BaseContentPage<AddServerViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="AddServerViewModel"/>.</param>
    public AddServerPage(AddServerViewModel viewModel)
        : base(viewModel)
	{
		InitializeComponent();
	}
}
