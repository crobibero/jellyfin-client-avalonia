namespace Jellyfin.Mvvm.ViewModels.Facades;

/// <summary>
/// A view model that handles back requests itself, e.g. to close an overlay.
/// </summary>
public interface IBackHandler
{
    /// <summary>
    /// Handle a back request.
    /// </summary>
    /// <returns><c>true</c> when the request was handled and navigation should not happen.</returns>
    Task<bool> HandleBackAsync();
}
