using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Facades;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Navigation service interface.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Initialize the root navigation page.
    /// </summary>
    /// <param name="window">The main window.</param>
    void Initialize(Window window);

    /// <summary>
    /// Navigate to the main view.
    /// </summary>
    void NavigateHome();

    /// <summary>
    /// Navigate to the login page.
    /// </summary>
    void NavigateToLoginPage();

    /// <summary>
    /// Navigate to the item's page.
    /// </summary>
    /// <param name="itemKind">The item kind.</param>
    /// <param name="itemId">The item id.</param>
    void NavigateToItemPage(BaseItemKind itemKind, Guid itemId);

    /// <summary>
    /// Navigate to the view.
    /// </summary>
    /// <typeparam name="TPage">The view type.</typeparam>
    void Navigate<TPage>()
        where TPage : Page;

    /// <summary>
    /// Navigate to the view, passing an id.
    /// </summary>
    /// <typeparam name="TPage">The view type.</typeparam>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="id">The id.</param>
    void Navigate<TPage, TViewModel>(Guid id)
        where TViewModel : BaseIdViewModel
        where TPage : BaseContentIdPage<TViewModel>;
}
