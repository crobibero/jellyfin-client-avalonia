using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// The add server page.
/// </summary>
public partial class ItemPage : BaseContentIdPage<ItemViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="ItemViewModel"/>.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{ItemPage}"/>.</param>
    public ItemPage(ItemViewModel viewModel, ILogger<ItemPage> logger)
        : base(viewModel, logger)
    {
        InitializeComponent();
    }
}
