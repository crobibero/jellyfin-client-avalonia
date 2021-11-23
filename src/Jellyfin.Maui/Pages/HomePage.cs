using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Home page.
/// </summary>
public class HomePage : BaseContentPage<HomeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="HomeViewModel"/>.</param>
    public HomePage(HomeViewModel viewModel)
        : base(viewModel, "Home")
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
                    new Label{Text = "Libraries"},
                    new CollectionView
                        {
                            ItemTemplate = new BaseItemDtoTemplate(),
                            ItemsLayout = LinearItemsLayout.Horizontal,
                            SelectionMode = SelectionMode.Single,
                        }
                        .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.LibrariesCollection))
                        .Bind(SelectableItemsView.SelectedItemProperty, nameof(ViewModel.SelectedItem))
                        .Bind(SelectableItemsView.SelectionChangedCommandProperty, nameof(ViewModel.NavigateCommand)),

                    new Label{Text = "Continue Watching"},
                    new CollectionView
                        {
                            ItemTemplate = new BaseItemDtoTemplate(),
                            ItemsLayout = LinearItemsLayout.Horizontal,
                            SelectionMode = SelectionMode.Single
                        }
                        .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.ContinueWatchingCollection))
                        .Bind(SelectableItemsView.SelectedItemProperty, nameof(ViewModel.SelectedItem))
                        .Bind(SelectableItemsView.SelectionChangedCommandProperty, nameof(ViewModel.NavigateCommand)),
                    new CollectionView
                        {
                            ItemTemplate = new RecentlyAddedTemplate(ViewModel.NavigateCommand, ViewModel.SelectedItem),
                            ItemsLayout = LinearItemsLayout.Vertical
                        }
                        .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.RecentlyAddedCollection))
                        .Bind(SelectableItemsView.SelectedItemProperty, nameof(ViewModel.SelectedItem))
                        .Bind(SelectableItemsView.SelectionChangedCommandProperty, nameof(ViewModel.NavigateCommand))
                }
            }
        };
    }
}
