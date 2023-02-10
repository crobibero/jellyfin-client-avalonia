using Maui.BindableProperty.Generator.Core;
using UraniumUI.Views;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Direct <see cref="DataTemplate"/> view.
/// </summary>
public partial class DataTemplateView : StatefulContentView
{
#pragma warning disable CS0169 // Used in source generators.
    [AutoBindable(OnChanged = nameof(BuildItem))]
    private DataTemplate? _itemTemplate;

    [AutoBindable(OnChanged = nameof(BuildItem))]
    private object? _item;

    [AutoBindable(OnChanged = nameof(BuildItem))]
    private bool _hideOnNullContent = true;
#pragma warning restore CS0169

    private void BuildItem()
    {
        if (Item is null || ItemTemplate is null)
        {
            Content = null;
            return;
        }

        // Create the content
        try
        {
            Content = CreateTemplateForItem(Item, ItemTemplate, false);
        }
        catch
        {
            Content = null;
        }
        finally
        {
            if (HideOnNullContent)
            {
                IsVisible = Content is not null;
            }
        }
    }

    private static View? CreateTemplateForItem(object item, DataTemplate itemTemplate, bool createDefaultIfNoTemplate = true)
    {
        // Check to see if we have a template selector or just a template
        var templateToUse = itemTemplate is DataTemplateSelector templateSelector
            ? templateSelector.SelectTemplate(item, null)
            : itemTemplate;

        // If we still don't have a template, create a label
        if (templateToUse is null)
        {
            return createDefaultIfNoTemplate
                ? new Label() { Text = item.ToString() }
                : null;
        }

        // Create the content
        // If a view wasn't created, we can't use it, exit.
        if (templateToUse.CreateContent() is not View view)
        {
            return new Label() { Text = item.ToString() };
        }

        // Set the binding
        view.BindingContext = item;

        return view;
    }
}
