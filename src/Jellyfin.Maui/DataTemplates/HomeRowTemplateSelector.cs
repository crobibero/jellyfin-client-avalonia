using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// Home row template selector.
/// </summary>
public class HomeRowTemplateSelector : DataTemplateSelector
{
    /// <inheritdoc />
    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is HomeRowModel homeRowModel)
        {
            // TODO switch based on item type.
            return TemplateHelper.LibraryHomeRowTemplate;
        }

        return null;
    }
}
