using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Navigation service interface.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Initialize the application.
    /// </summary>
    /// <param name="application">The application.</param>
    void Initialize(Application application);

    /// <summary>
    /// Navigate to the main view.
    /// </summary>
    void NavigateHome();

    /// <summary>
    /// Navigate to the login page.
    /// </summary>
    void NavigateToLoginPage();

    /// <summary>
    /// Navigate to the server selection page.
    /// </summary>
    void NavigateToServerSelectPage();

    /// <summary>
    /// Navigate to the add server page.
    /// </summary>
    void NavigateToAddServerPage();

    /// <summary>
    /// Navigate to the item's view.
    /// </summary>
    /// <param name="itemKind">The item kind.</param>
    /// <param name="itemId">The item id.</param>
    void NavigateToItemView(BaseItemKind itemKind, Guid itemId);
}
