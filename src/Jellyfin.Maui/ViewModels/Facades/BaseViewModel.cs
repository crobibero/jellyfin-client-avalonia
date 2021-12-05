using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Jellyfin.Maui.ViewModels.Facades;

/// <summary>
/// Base view model.
/// </summary>
public abstract class BaseViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
    /// </summary>
    protected BaseViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Gets the cancellation token which denotes the view model has been disposed.
    /// </summary>
    protected CancellationToken ViewModelCancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Initialize the view model.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Ensure Observable Collection is thread-safe.
    /// </summary>
    /// <remarks>
    ///  https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/.
    /// </remarks>
    /// <param name="collection">The collection.</param>
    /// <param name="context">The context.</param>
    /// <param name="accessMethod">The access method.</param>
    /// <param name="writeAccess">Whether to enable write access.</param>
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

    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose all resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
