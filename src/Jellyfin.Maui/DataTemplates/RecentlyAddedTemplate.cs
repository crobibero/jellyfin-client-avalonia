using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.DataTemplates;

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

        private static VerticalStackLayout Initialize() =>
            new()
            {
                Children =
                {
                    new Label()
                        .Bind(Label.TextProperty, nameof(RecentlyAddedModel.LibraryName)),
                    new CollectionView
                        {
                            ItemTemplate = new PosterCardTemplate(),
                            ItemsLayout = LinearItemsLayout.Horizontal,
                            SelectionMode = SelectionMode.Single
                        }
                        .Bind(ItemsView.ItemsSourceProperty, nameof(RecentlyAddedModel.Items))
                }
            };
    }
