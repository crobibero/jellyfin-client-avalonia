namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// Card template selector.
/// </summary>
public class CardTemplateSelector : DataTemplateSelector
{
    /// <inheritdoc/>
    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is BaseItemDto baseItemDto)
        {
            return baseItemDto.Type switch
            {
                BaseItemKind.CollectionFolder => TemplateHelper.LibraryCardTemplate,
                _ => TemplateHelper.PosterCardTemplate,
            };
        }

        return null;
    }
}
