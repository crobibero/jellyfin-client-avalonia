namespace Jellyfin.Mvvm.Services;

/// <summary>
/// How a view model was navigated to.
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// The view model was newly created.
    /// </summary>
    New = 0,

    /// <summary>
    /// The user navigated back to the view model.
    /// </summary>
    Back = 1
}
