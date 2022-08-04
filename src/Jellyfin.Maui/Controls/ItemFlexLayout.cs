using System.Collections;
using System.Collections.Specialized;
using Maui.BindableProperty.Generator.Core;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Flex layout with dynamic items.
/// This shouldn't be needed after rc2.
/// </summary>
public partial class ItemFlexLayout : FlexLayout
{
    [AutoBindable(OnChanged = nameof(OnItemsSourceChanged))]
    private IEnumerable? _itemsSource;

    [AutoBindable]
    private DataTemplate? _itemTemplate;

    private void OnItemsSourceChanged(IEnumerable newVal)
    {
        if (newVal is INotifyCollectionChanged observableCollection)
        {
            observableCollection.CollectionChanged += OnItemsSourceCollectionChanged;
        }

        Children.Clear();
        if (newVal is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                Children.Add(CreateChildView(item));
            }
        }
    }

    private View CreateChildView(object item)
    {
        ArgumentNullException.ThrowIfNull(ItemTemplate);

        if (ItemTemplate is DataTemplateSelector dts)
        {
            var itemTemplate = dts.SelectTemplate(item, null);
            itemTemplate.SetValue(BindingContextProperty, item);
            return (View)itemTemplate.CreateContent();
        }
        else
        {
            ItemTemplate.SetValue(BindingContextProperty, item);
            return (View)ItemTemplate.CreateContent();
        }
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not IList list)
        {
            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Children.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item is not null)
                {
                    Children.Add(CreateChildView(item));
                }
            }
        }
        else
        {
            if (e.OldItems != null)
            {
                Children.RemoveAt(e.OldStartingIndex);
            }

            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var item = e.NewItems[i];
                    if (item is not null)
                    {
                        var view = CreateChildView(item);
                        Children.Insert(e.NewStartingIndex + i, view);
                    }
                }
            }
        }
    }
}
