using System.Collections;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Navigation service interface.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate to the main page.
    /// </summary>
    void NavigateToMainPage();

    /// <summary>
    /// Navigate to the server selection page.
    /// </summary>
    void NavigateToServerSelectPage();

    /// <summary>
    /// Navigate to the add server page.
    /// </summary>
    void NavigateToAddServerPage();

    /// <summary>
    /// Navigate to the user select page.
    /// </summary>
    void NavigateToUserSelectPage();

    /// <summary>
    /// Navigate to the login page.
    /// </summary>
    void NavigateToLoginPage();

    /// <summary>
    /// Navigate to the main view.
    /// </summary>
    void NavigateHome();

    /// <summary>
    /// Navigate to the item's view.
    /// </summary>
    /// <param name="item">The item.</param>
    void NavigateToItemView(BaseItemDto item);

    /// <summary>
    /// Schedules the provided action on the UI thread from a worker thread.
    /// </summary>
    /// <returns>true when the action has been dispatched successfully, otherwise false.</returns>
    /// <param name="action">he method to be executed by the dispatcher.</param>
    bool Dispatch(Action action);

    /// <summary>
    /// Schedules the provided action on the UI thread from a worker thread.
    /// </summary>
    /// <returns>true when the action has been dispatched successfully, otherwise false.</returns>
    /// <param name="action">he method to be executed by the dispatcher.</param>
    Task DispatchAsync(Action action);

    /// <summary>
    /// Starts synchronization on the collection by using callback and context.
    /// </summary>
    /// <param name="collection">The collection that will be read or updated.</param>
    /// <param name="context">The context or lock object that will be passed to callback. May be null.</param>
    /// <param name="callback">The synchronization callback.</param>
    void EnableCollectionSynchronization(IEnumerable collection, object? context, Action<IEnumerable, object, Action, bool> callback);
}
