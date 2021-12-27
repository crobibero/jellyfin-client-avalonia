using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.DataTemplates
{
    /// <summary>
    /// DataTemplate for BaseItem.
    /// </summary>
    public class RecentlyAddedTemplate : DataTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecentlyAddedTemplate"/> class.
        /// </summary>
        public RecentlyAddedTemplate()
            : base(Initialize)
        {
        }

        private enum Row
        {
            Title,
            Items
        }

        private static Grid Initialize() =>
            new()
            {
                RowSpacing = 1,
                RowDefinitions = GridRowsColumns.Rows.Define(
                    (Row.Title, new GridLength(1, GridUnitType.Star)),
                    (Row.Items, new GridLength(9, GridUnitType.Star))),
                Children =
                {
                    new Label()
                        .Row(Row.Title)
                        .Bind(Label.TextProperty, nameof(RecentlyAddedModel.LibraryName)),
                    new CollectionView
                        {
                            ItemTemplate = new PosterCardTemplate(),
                            ItemsLayout = LinearItemsLayout.Horizontal,
                            SelectionMode = SelectionMode.Single
                        }
                        .Row(Row.Items)
                        .Bind(ItemsView.ItemsSourceProperty, nameof(RecentlyAddedModel.Items))
                }
            };
    }
}
