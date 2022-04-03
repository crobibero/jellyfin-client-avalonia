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
            if (baseItemDto.Type == BaseItemKind.CollectionFolder)
            {
                // TODO.
            }

            return TemplateHelper.PosterCardTemplate;
        }

        return null;
    }
}
