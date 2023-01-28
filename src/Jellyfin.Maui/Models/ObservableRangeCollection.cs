using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Jellyfin.Maui.Models;

/// <summary>
/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The type of collection.</typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableRangeCollection{T}"/> class.
    /// </summary>
    public ObservableRangeCollection()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableRangeCollection{T}"/> class.
    /// </summary>
    /// <param name="collection">collection: The collection from which the elements are copied.</param>
    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
    /// </summary>
    /// <param name="collection">The collection to add.</param>
    /// <param name="notificationMode">The notification changed action.</param>
    public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        if (notificationMode is not NotifyCollectionChangedAction.Add and not NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
        }

        ArgumentNullException.ThrowIfNull(nameof(collection));

        CheckReentrancy();

        var startIndex = Count;

        var itemsAdded = AddArrangeCore(collection);

        if (!itemsAdded)
        {
            return;
        }

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);

            return;
        }

        var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Add,
            changedItems: changedItems,
            startingIndex: startIndex);
    }

    /// <summary>
    /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T).
    /// </summary>
    /// <remarks>
    /// With notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
    /// </remarks>
    /// <param name="collection">The collection to remove.</param>
    /// <param name="notificationMode">The notification mode.</param>
    public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
    {
        if (notificationMode is not NotifyCollectionChangedAction.Remove and not NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
        }

        ArgumentNullException.ThrowIfNull(nameof(collection));

        CheckReentrancy();

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            var raiseEvents = false;
            foreach (var item in collection)
            {
                Items.Remove(item);
                raiseEvents = true;
            }

            if (raiseEvents)
            {
                RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
            }

            return;
        }

        var changedItems = new List<T>(collection);
        for (var i = 0; i < changedItems.Count; i++)
        {
            if (!Items.Remove(changedItems[i]))
            {
                // Can't use a foreach because changedItems is intended to be (carefully) modified
                changedItems.RemoveAt(i);
                i--;
            }
        }

        if (changedItems.Count == 0)
        {
            return;
        }

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Remove,
            changedItems: changedItems);
    }

    /// <summary>
    /// Clears the current collection and replaces it with the specified item.
    /// </summary>
    /// <param name="item">The item to replace with.</param>
    public void Replace(T item) => ReplaceRange(new T[] { item });

    /// <summary>
    /// Clears the current collection and replaces it with the specified collection.
    /// </summary>
    /// <param name="collection">The collection to replace with.</param>
    public void ReplaceRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(nameof(collection));

        CheckReentrancy();

        var previouslyEmpty = Items.Count == 0;

        Items.Clear();

        AddArrangeCore(collection);

        var currentlyEmpty = Items.Count == 0;

        if (previouslyEmpty && currentlyEmpty)
        {
            return;
        }

        RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
    }

    private bool AddArrangeCore(IEnumerable<T> collection)
    {
        var itemAdded = false;
        foreach (var item in collection)
        {
            Items.Add(item);
            itemAdded = true;
        }

        return itemAdded;
    }

    private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T>? changedItems = null, int startingIndex = -1)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        if (changedItems is null)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
        }
        else
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems: changedItems, startingIndex: startingIndex));
        }
    }
}
