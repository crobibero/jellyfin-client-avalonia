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
        Content = new VerticalStackLayout
        {
            WidthRequest = 50,
            HeightRequest = 100,
            Children =
                {
                    new Image()
                        .Bind(Image.SourceProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardPosterConverter(ImageType.Primary)),
                    new Label()
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardTitleConverter()),
                    new Label()
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardDescriptionConverter()),
                }
        };
    }
}
