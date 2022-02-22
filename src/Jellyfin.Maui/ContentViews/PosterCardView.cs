using Jellyfin.Maui.Converters;
using Jellyfin.Maui.ViewModels.Facades;

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
        }
        .BindClickGesture(
            commandPath: nameof(BaseViewModel.NavigateToItemCommand),
            commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(BaseViewModel)),
            parameterPath: ".",
            parameterSource: Item)
        .BindTapGesture(
            commandPath: nameof(BaseViewModel.NavigateToItemCommand),
            commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(BaseViewModel)),
            parameterPath: ".",
            parameterSource: Item);
    }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    public BaseItemDto? Item { get; set; }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        Item = (BaseItemDto?)BindingContext;
        base.OnBindingContextChanged();
    }
}
