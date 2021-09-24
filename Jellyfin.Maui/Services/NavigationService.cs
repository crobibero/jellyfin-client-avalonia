using System;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Services
{
    internal class NavigationService : INavigationService
    {
        private ContentPage? _rootContentPage;

        public void Initialize(ContentPage rootContentPage)
        {
            _rootContentPage = rootContentPage;
        }

        public void NavigateRoot<T>()
            where T : View
        {
            var resolvedView = ServiceProvider.GetService<T>();
            _rootContentPage.Content = resolvedView;
        }
    }
}
