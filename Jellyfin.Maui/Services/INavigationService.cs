using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Services
{
    /// <summary>
    /// Navigation service.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Initialize the root content page.
        /// </summary>
        /// <param name="rootContentPage">The root page.</param>
        void Initialize(ContentPage rootContentPage);

        /// <summary>
        /// Navigate to the provided view.
        /// </summary>
        /// <typeparam name="T">The view type.</typeparam>
        void NavigateRoot<T>()
            where T : View;
    }
}
