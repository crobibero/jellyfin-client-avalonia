using System.Collections;
using System.Collections.Specialized;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// Flex layout with dynamic items.
/// </summary>
public class ItemFlexLayout : FlexLayout
{
    /// <summary>
    /// The <see cref="ItemsSource"/> property.
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(ItemFlexLayout),
        propertyChanged: OnItemsSourceChanged);

    /// <summary>
    /// The <see cref="ItemTemplate"/> property.
    /// </summary>
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate),
        typeof(DataTemplate),
        typeof(ItemFlexLayout));

    /// <summary>
    /// Gets or sets the list of items.
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the item template.
    /// </summary>
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldVal, object newVal)
    {
        var layout = (ItemFlexLayout)bindable;

        if (newVal is INotifyCollectionChanged observableCollection)
        {
            observableCollection.CollectionChanged += layout.OnItemsSourceCollectionChanged;
        }

        layout.Children.Clear();
        if (newVal is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                layout.Children.Add(layout.CreateChildView(item));
            }
        }
    }

    private View CreateChildView(object item)
    {
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
