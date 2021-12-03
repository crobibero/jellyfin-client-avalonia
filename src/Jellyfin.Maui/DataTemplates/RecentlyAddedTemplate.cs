using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Models;
using Microsoft.Maui;

namespace Jellyfin.Maui.DataTemplates
{
    /// <summary>
    /// DataTemplate for BaseItem.
    /// </summary>
    public class RecentlyAddedTemplate : DataTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecentlyAddedTemplate"/>.
        /// </summary>
        public RecentlyAddedTemplate()
            : base(Initialize)
        {
        }

        private static Grid Initialize() =>
            new()
            {
                RowSpacing = 1,
                RowDefinitions = GridRowsColumns.Rows.Define
                (
                    (Row.Title, new GridLength(1, GridUnitType.Star)),
                    (Row.Items, new GridLength(9, GridUnitType.Star))
                ),
                Children =
                {
                    new Label()
                        .Row(Row.Title)
                        .Bind(Label.TextProperty, nameof(RecentlyAddedModel.Name)),
                    new CollectionView
                        {
                            ItemTemplate = new BaseItemDtoTemplate(),
                            ItemsLayout = LinearItemsLayout.Horizontal,
                            SelectionMode = SelectionMode.Single
                        }
                        .Row(Row.Items)
                        .Bind(ItemsView.ItemsSourceProperty, nameof(RecentlyAddedModel.Items))
                }
            };

        private enum Row
        {
            Title,
            Items
        }
    }
}
