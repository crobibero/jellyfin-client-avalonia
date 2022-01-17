using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Converters;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Poster card view.
/// </summary>
public class PosterCardView : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PosterCardView"/> class.
    /// </summary>
    public PosterCardView()
    {
        Content = new Grid
        {
            WidthRequest = 50,
            HeightRequest = 100,
            RowSpacing = 1,
            RowDefinitions = GridRowsColumns.Rows.Define(
                    (Row.Poster, new GridLength(8, GridUnitType.Star)),
                    (Row.Title, new GridLength(1, GridUnitType.Star)),
                    (Row.Description, new GridLength(1, GridUnitType.Star))),
            Children =
                {
                    new Image()
                        .Row(Row.Poster)
                        .Bind(Image.SourceProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardPosterConverter(ImageType.Primary)),
                    new Label()
                        .Row(Row.Title)
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardTitleConverter()),
                    new Label()
                        .Row(Row.Description)
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardDescriptionConverter()),
                }
        };
    }

    private enum Row
    {
        Poster = 0,
        Title = 1,
        Description = 2
    }
}
