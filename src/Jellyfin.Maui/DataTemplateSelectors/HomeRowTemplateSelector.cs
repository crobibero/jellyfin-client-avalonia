using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// Home row template selector.
/// </summary>
public class HomeRowTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets or sets the library home row template.
    /// </summary>
    public DataTemplate? LibraryHomeRowTemplate { get; set; }

    /// <inheritdoc />
    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        if (item is HomeRowModel)
        {
            // TODO switch based on item type.
            return LibraryHomeRowTemplate;
        }

        return null;
    }
}
