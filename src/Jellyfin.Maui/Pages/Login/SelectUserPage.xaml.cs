using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels.Login;
using Microsoft.Extensions.Logging;

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
    /// <param name="logger">Instance of the <see cref="ILogger{SelectUserPage}"/>.</param>
    public SelectUserPage(SelectUserViewModel viewModel, ILogger<SelectUserPage> logger)
        : base(viewModel, logger)
    {
        InitializeComponent();
    }
}
