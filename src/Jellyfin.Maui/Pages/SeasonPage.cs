using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Season page.
/// </summary>
public class SeasonPage : BaseContentIdPage<SeasonViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SeasonViewModel"/>.</param>
    public SeasonPage(SeasonViewModel viewModel)
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
                    .Bind(Label.TextProperty, "Item.Name", mode: BindingMode.OneWay),
                new Label { Text = Strings.Episodes },
                new CollectionView
                {
                    ItemTemplate = TemplateHelper.PosterCardTemplate,
                    ItemsLayout = LinearItemsLayout.Horizontal,
                    SelectionMode = SelectionMode.Single,
                    ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView
                }
                .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.EpisodeCollection))
                // .Bind(SelectableItemsView.SelectedItemProperty, nameof(ViewModel.SelectedItem))
                // .Bind(SelectableItemsView.SelectionChangedCommandProperty, nameof(ViewModel.NavigateToItemCommand))
            }
        };
    }
}
