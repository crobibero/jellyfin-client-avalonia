using System;
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

        public void Navigate<T>(Guid id)
            where T : Page, IInitializeId
        {
            var resolvedView = ServiceProvider.GetService<T>();
            resolvedView.Initialize(id);

            Application.Current!.Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PushAsync(resolvedView).ConfigureAwait(true);
            });
        }

        public void Navigate<T>()
            where T : Page
        {
            var resolvedView = ServiceProvider.GetService<T>();

            Application.Current!.Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PushAsync(resolvedView).ConfigureAwait(true);
            });
        }

        public void NavigateToMain()
        {
            Application.Current!.Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PopToRootAsync().ConfigureAwait(true);
            });
        }
    }
}
