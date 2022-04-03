using Jellyfin.Maui.ContentViews.Facades;
using Jellyfin.Maui.Converters;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Poster card view.
/// </summary>
public class PosterCardView : BaseContentView<BaseItemDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PosterCardView"/> class.
    /// </summary>
    public PosterCardView()
    {
        Content = new VerticalStackLayout
        {
            Style = BaseStyles.PosterCard,
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
            parameterSource: Context)
        .BindTapGesture(
            commandPath: nameof(BaseViewModel.NavigateToItemCommand),
            commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(BaseViewModel)),
            parameterPath: ".",
            parameterSource: Context);
    }
}
