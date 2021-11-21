using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Base view model.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    /// <summary>
    /// Initialize the view model.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Ensure Observable Collection is thread-safe https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/.
    /// </summary>
    protected static void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
    {
        Device.BeginInvokeOnMainThread(accessMethod);
    }

    /// <summary>
    /// Insert the model into the collection.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="comparison">The comparison.</param>
    /// <param name="modelToInsert">The model to insert.</param>
    /// <typeparam name="T">The type of model.</typeparam>
    protected void InsertIntoSortedCollection<T>(ObservableCollection<T> collection, Comparison<T> comparison, T modelToInsert)
    {
        if (collection.Count is 0)
        {
            collection.Add(modelToInsert);
        }
        else
        {
            int index = 0;
            foreach (var model in collection)
            {
                if (comparison(model, modelToInsert) >= 0)
                {
                    collection.Insert(index, modelToInsert);
                    return;
                }

                index++;
            }

            collection.Insert(index, modelToInsert);
        }
    }
}
