using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels.Login;
using Microsoft.Extensions.Logging;

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
    /// <param name="logger">Instance of the <see cref="ILogger{SelectServerPage}"/>.</param>
    public SelectServerPage(ServerSelectViewModel viewModel, ILogger<SelectServerPage> logger)
        : base(viewModel, logger)
    {
        InitializeComponent();
    }
}
