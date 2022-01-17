using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Home page.
/// </summary>
public class HomePage : BaseContentPage<HomeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="HomeViewModel"/>.</param>
    public HomePage(HomeViewModel viewModel)
        : base(viewModel, Strings.Home)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new ScrollView
        {
            Content = new StackLayout
            {
                Padding = 16,
                Children =
                {
                    new CollectionView
                        {
                            ItemTemplate = new HomeRowTemplateSelector(),
                            ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
                            ItemsLayout = LinearItemsLayout.Vertical
                        }
                        .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.HomeRowCollection))
                }
            }
        };
    }
}
