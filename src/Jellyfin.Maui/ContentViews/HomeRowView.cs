using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Home row view.
/// </summary>
public class HomeRowView : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeRowView"/> class.
    /// </summary>
    public HomeRowView()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label()
                    .Bind(Label.TextProperty, mode: BindingMode.OneTime, path: nameof(HomeRowModel.Name)),
                new CollectionView
                    {
                        ItemTemplate = new PosterCardTemplate(),
                        ItemsLayout = LinearItemsLayout.Horizontal,
                        ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView
                    }
                    .Bind(ItemsView.ItemsSourceProperty, mode: BindingMode.OneTime, path: nameof(HomeRowModel.Items))
            }
        };
    }
}
