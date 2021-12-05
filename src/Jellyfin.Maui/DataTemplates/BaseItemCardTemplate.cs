using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Converters;
using Microsoft.Maui;

namespace Jellyfin.Maui.DataTemplates
{
    /// <summary>
    /// DataTemplate for BaseItem.
    /// </summary>
    public class BaseItemCardTemplate : DataTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemCardTemplate"/> class.
        /// </summary>
        public BaseItemCardTemplate()
            : base(Initialize)
        {
        }

        private enum Row
        {
            Poster = 0,
            Title = 1,
            Description = 2
        }

        private static Grid Initialize() =>
            new()
            {
                RowSpacing = 1,
                RowDefinitions = GridRowsColumns.Rows.Define(
                    (Row.Poster, new GridLength(8, GridUnitType.Star)),
                    (Row.Title, new GridLength(1, GridUnitType.Star)),
                    (Row.Description, new GridLength(1, GridUnitType.Star))),
                Children =
                {
                    new Image()
                        .Row(Row.Poster)
                        .Bind(Image.SourceProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardPosterConverter()),
                    new Label()
                        .Row(Row.Title)
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardTitleConverter()),
                    new Label()
                        .Row(Row.Description)
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardDescriptionConverter()),
                }
            };
    }
}
