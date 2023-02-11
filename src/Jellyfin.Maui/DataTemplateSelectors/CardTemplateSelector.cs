namespace Jellyfin.Maui.DataTemplateSelectors;

/// <summary>
/// Card template selector.
/// </summary>
public class CardTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets or sets the library card template.
    /// </summary>
    public DataTemplate? LibraryCardTemplate { get; set; }

    /// <summary>
    /// Gets or sets the poster card template.
    /// </summary>
    public DataTemplate? PosterCardTemplate { get; set; }

    /// <inheritdoc/>
    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is BaseItemDto baseItemDto)
        {
            return baseItemDto.Type switch
            {
                BaseItemKind.CollectionFolder => LibraryCardTemplate,
                _ => PosterCardTemplate,
            };
        }

        return null;
    }
}
