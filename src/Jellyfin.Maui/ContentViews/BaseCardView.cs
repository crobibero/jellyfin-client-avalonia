using Jellyfin.Maui.ContentViews.Facades;
using Jellyfin.Maui.Converters;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Poster card view.
/// </summary>
public class BaseCardView : BaseContentView<BaseItemDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseCardView"/> class.
    /// </summary>
    /// <param name="cardStyle">The style to apply to the card.</param>
    /// <param name="imageStyle">The style to apply to the image.</param>
    /// <param name="titleStyle">The style to apply to the title.</param>
    /// <param name="descriptionStyle">The style to apply to the description.</param>
    /// <param name="imageType">The image type to use.</param>
    /// <param name="rowDefinitions">The row definitions for the card.</param>
    protected BaseCardView(
        Style? cardStyle,
        Style? imageStyle,
        Style? titleStyle,
        Style? descriptionStyle,
        ImageType imageType,
        (Row, GridLength)[] rowDefinitions)
    {
        Content = new Grid
        {
            Style = cardStyle,
            RowDefinitions = GridRowsColumns.Rows.Define(rowDefinitions),
            Children =
                {
                    new Image
                    {
                        Style = imageStyle
                    }
                    .Row(Row.Image)
                    .Bind(Image.SourceProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardPosterConverter(imageType)),
                    new Label
                    {
                        Style = titleStyle
                    }
                    .Row(Row.Title)
                    .Bind(Label.TextProperty, mode: BindingMode.OneTime, converter: new BaseItemDtoCardTitleConverter()),
                    new Label
                    {
                        Style = descriptionStyle
                    }
                    .Row(Row.Description)
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

    /// <summary>
    /// Row definition.
    /// </summary>
    protected enum Row
    {
        /// <summary>
        /// Image row.
        /// </summary>
        Image = 0,

        /// <summary>
        /// Title row.
        /// </summary>
        Title = 1,

        /// <summary>
        /// Description row.
        /// </summary>
        Description = 2
    }
}
