using Avalonia;
using Avalonia.iOS;
using Foundation;
using Jellyfin.Avalonia.LibVlc;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.iOS;

/// <summary>
/// The application delegate, configuring Avalonia and the platform services.
/// </summary>
[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
    /// <inheritdoc />
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        App.ConfigurePlatformServices = services =>
        {
            services.AddSingleton<IDeviceInfo, AppleDeviceInfo>();
            services.AddSingleton<IPlayerBackend, AppleLibVlcBackend>();
            services.AddSingleton<IVideoViewFactory, LibVlcVideoViewFactory>();
        };

        return base.CustomizeAppBuilder(builder).WithFigtreeFont();
    }
}
