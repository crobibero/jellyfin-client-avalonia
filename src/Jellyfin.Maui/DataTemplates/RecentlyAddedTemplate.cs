using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.Input;
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
        public RecentlyAddedTemplate(
            IRelayCommand selectionChangedCommand,
            object? selectedItem)
            : base(Initialize)
        {
            SelectedItem = selectedItem;
            SelectionChangedCommand = selectionChangedCommand;
        }

        /// <summary>
        /// Gets or sets the selected item path.
        /// </summary>
        public object? SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the upstream selection changed command.
        /// </summary>
        public IRelayCommand SelectionChangedCommand { get; set; }

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
                        .Bind(SelectableItemsView.SelectedItemProperty, nameof(SelectedItem))
                        .Bind(SelectableItemsView.SelectionChangedCommandProperty, nameof(SelectionChangedCommand)),
                }
            };

        private enum Row
        {
            Title,
            Items
        }
    }
}
