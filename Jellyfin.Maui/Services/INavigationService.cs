using Jellyfin.Maui.Pages;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Navigation service interface.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Initialize the root navigation page.
    /// </summary>
    /// <param name="navigationPage">The root navigation page.</param>
    void Initialize(NavigationPage navigationPage);

    /// <summary>
    /// Navigate to the main view.
    /// </summary>
    void NavigateToMain();

    /// <summary>
    /// Navigate to the view.
    /// </summary>
    /// <typeparam name="T">The view type.</typeparam>
    void Navigate<T>()
        where T : Page;

    /// <summary>
    /// Navigate to the view, passing an id.
    /// </summary>
    /// <typeparam name="T">The view type.</typeparam>
    /// <param name="id">The id.</param>
    void Navigate<T>(Guid id)
        where T : Page, IInitializeId;
}
