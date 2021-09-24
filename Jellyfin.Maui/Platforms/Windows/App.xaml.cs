using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Jellyfin.Maui.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <remarks>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </remarks>
        public App()
        {
            this.InitializeComponent();
        }

        /// <inheritdoc/>
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            Microsoft.Maui.Essentials.Platform.OnLaunched(args);

            var version = typeof(MauiProgram).Assembly.GetName().Version?.ToString() ?? "0.0.0.1";
            this.Services.GetRequiredService<SdkClientSettings>()
               .InitializeClientSettings(
               "Jellyfin Maui Windows",
               version,
               "Windows",
               Guid.NewGuid().ToString("N"));
        }
    }
}
