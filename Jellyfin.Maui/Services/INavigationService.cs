using System;
using System.Threading.Tasks;
using Jellyfin.Maui.Pages;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Services
{
    /// <summary>
    /// Navigation service.
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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NavigateToMainAsync();

        /// <summary>
        /// Navigate to the view.
        /// </summary>
        /// <typeparam name="T">The view type.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NavigateAsync<T>()
            where T : Page;

        /// <summary>
        /// Navigate to the view, passing an id.
        /// </summary>
        /// <typeparam name="T">The view type.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NavigateAsync<T>(Guid id)
            where T : Page, IInitializeId;
    }
}
