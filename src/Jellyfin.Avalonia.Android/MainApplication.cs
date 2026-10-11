using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Jellyfin.Avalonia.LibVlc;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Android;

/// <summary>
/// The Android application, configuring Avalonia and the platform services.
/// </summary>
[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainApplication"/> class.
    /// </summary>
    /// <param name="javaReference">The java reference.</param>
    /// <param name="transfer">The ownership transfer.</param>
    protected MainApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    /// <inheritdoc />
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        App.ConfigurePlatformServices = services =>
        {
            services.AddSingleton<IDeviceInfo, AndroidDeviceInfo>();
            services.AddSingleton<IPlayerBackend, AndroidPlayerBackend>();
            services.AddSingleton<IVideoViewFactory, LibVlcVideoViewFactory>();
        };

        return base.CustomizeAppBuilder(builder).WithFigtreeFont();
    }
}
