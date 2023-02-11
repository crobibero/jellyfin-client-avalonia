using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels;

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
    public ItemPage(ItemViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
