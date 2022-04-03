using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Item page.
/// </summary>
public class ItemPage : BaseContentIdPage<ItemViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="ItemViewModel"/>.</param>
    public ItemPage(ItemViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new StackLayout
        {
            Padding = 16,
            Children =
            {
                new Label()
                    .Bind(Label.TextProperty, $"{nameof(ViewModel.Item)}.{nameof(ViewModel.Item.Name)}", mode: BindingMode.OneWay),
            }
        };
    }
}
