namespace Jellyfin.Maui.Services;

/// <summary>
/// Navigation service interface.
/// </summary>
public interface INavigationService
{
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
}
