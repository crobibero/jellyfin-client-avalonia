using System.Collections;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class ApplicationService : IApplicationService
{
    /// <inheritdoc />
    public bool Dispatch(Action action) => Application.Current!.Dispatcher.Dispatch(action);

    /// <inheritdoc />
    public Task DispatchAsync(Action action) => Application.Current!.Dispatcher.DispatchAsync(action);

    /// <inheritdoc />
    public void EnableCollectionSynchronization(IEnumerable collection, object? context, Action<IEnumerable, object, Action, bool> callback)
        => BindingBase.EnableCollectionSynchronization(collection, context, new CollectionSynchronizationCallback(callback));
}
