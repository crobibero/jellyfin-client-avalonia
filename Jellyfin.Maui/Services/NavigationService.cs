using System;
using System.Threading.Tasks;
using Jellyfin.Maui.Pages;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Services
{
    internal class NavigationService : INavigationService
    {
        // Navigation page is initialized on startup.
        private NavigationPage _navigationPage = null!;

        public void Initialize(NavigationPage navigationPage)
        {
            _navigationPage = navigationPage;
        }

        public async Task NavigateAsync<T>(Guid id)
            where T : Page, IInitializeId
        {
            var resolvedView = ServiceProvider.GetService<T>();
            resolvedView.Initialize(id);
            await _navigationPage!.PushAsync(resolvedView).ConfigureAwait(false);
        }

        public async Task NavigateAsync<T>()
            where T : Page
        {
            var resolvedView = ServiceProvider.GetService<T>();
            await _navigationPage!.PushAsync(resolvedView).ConfigureAwait(false);
        }

        public async Task NavigateToMainAsync()
        {
            await _navigationPage.PopToRootAsync().ConfigureAwait(false);
        }
    }
}
