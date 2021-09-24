using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Services;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace Jellyfin.Maui
{
    /// <summary>
    /// The main application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create a new window.
        /// </summary>
        /// <param name="activationState">The activation state.</param>
        /// <returns>The created window.</returns>
        protected override Window CreateWindow(Microsoft.Maui.IActivationState activationState)
        {
            var navigationService = ServiceProvider.GetService<INavigationService>();
            var mainPage = ServiceProvider.GetService<MainPage>();
            var navigationPage = new NavigationPage(mainPage);
            navigationService.Initialize(navigationPage);
            return new Window(navigationPage);
        }
    }
}
