namespace Jellyfin.Mvvm.Services;

/// <summary>
/// ApplicationService service interface.
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Schedules the provided action on the UI thread.
    /// </summary>
    /// <param name="action">The method to be executed by the dispatcher.</param>
    void Post(Action action);

    /// <summary>
    /// Runs the provided action on the UI thread.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <param name="action">The method to be executed by the dispatcher.</param>
    Task DispatchAsync(Action action);

    /// <summary>
    /// Gets a value indicating whether the caller is on the UI thread.
    /// </summary>
    /// <returns><c>true</c> when on the UI thread.</returns>
    bool IsUiThread();

    /// <summary>
    /// Exit the application, when supported by the platform.
    /// </summary>
    void Exit();
}
