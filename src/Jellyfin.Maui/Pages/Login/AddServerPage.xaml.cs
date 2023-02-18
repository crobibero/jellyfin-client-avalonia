using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels.Login;
using Microsoft.Extensions.Logging;

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
    /// <param name="logger">Instance of the <see cref="ILogger{AddServerPage}"/>.</param>
    public AddServerPage(AddServerViewModel viewModel, ILogger<AddServerPage> logger)
    : base(viewModel, logger)
    {
        InitializeComponent();
    }
}
