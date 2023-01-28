using System.Collections;

namespace Jellyfin.Maui.Services;

/// <summary>
/// ApplicationService service interface.
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Schedules the provided action on the UI thread from a worker thread.
    /// </summary>
    /// <returns>true when the action has been dispatched successfully, otherwise false.</returns>
    /// <param name="action">The method to be executed by the dispatcher.</param>
    bool Dispatch(Action action);

    /// <summary>
    /// Schedules the provided action on the UI thread from a worker thread.
    /// </summary>
    /// <returns>true when the action has been dispatched successfully, otherwise false.</returns>
    /// <param name="action">The method to be executed by the dispatcher.</param>
    Task DispatchAsync(Action action);

    /// <summary>
    /// Starts synchronization on the collection by using callback and context.
    /// </summary>
    /// <param name="collection">The collection that will be read or updated.</param>
    /// <param name="context">The context or lock object that will be passed to callback. May be null.</param>
    /// <param name="callback">The synchronization callback.</param>
    void EnableCollectionSynchronization(IEnumerable collection, object? context, Action<IEnumerable, object, Action, bool> callback);
}
