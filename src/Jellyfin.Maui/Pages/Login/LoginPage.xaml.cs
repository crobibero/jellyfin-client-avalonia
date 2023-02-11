using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The login page.
/// </summary>
public partial class LoginPage : BaseContentPage<LoginViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="LoginViewModel"/>.</param>
    public LoginPage(LoginViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
