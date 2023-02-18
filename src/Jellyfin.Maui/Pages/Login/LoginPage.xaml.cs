using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels.Login;
using Microsoft.Extensions.Logging;

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
    /// <param name="logger">Instance of the <see cref="ILogger{LoginPage}"/>.</param>
    public LoginPage(LoginViewModel viewModel, ILogger<LoginPage> logger)
        : base(viewModel, logger)
    {
        InitializeComponent();
    }
}
