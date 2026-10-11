using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using Jellyfin.Avalonia;
using Jellyfin.Avalonia.Browser;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Microsoft.Extensions.DependencyInjection;

[assembly: SupportedOSPlatform("browser")]

await JSHost.ImportAsync(BrowserInterop.PlayerModule, "../player.js");

App.ConfigurePlatformServices = services =>
{
    services.AddSingleton<ISettingsService, BrowserSettingsService>();
    services.AddSingleton<IDeviceInfo, BrowserDeviceInfo>();
    services.AddSingleton<IPlayerBackend, BrowserPlayerBackend>();
    services.AddSingleton<IVideoViewFactory, BrowserVideoViewFactory>();
};

await AppBuilder.Configure<App>()
    .WithFigtreeFont()
    .StartBrowserAppAsync("out");

// The canvas must be able to show the video element behind it.
DispatcherTimer.Run(
    () =>
    {
        var view = (Application.Current?.ApplicationLifetime as ISingleViewApplicationLifetime)?.MainView;
        if (view is null || TopLevel.GetTopLevel(view) is not { } topLevel)
        {
            return true;
        }

        topLevel.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        topLevel.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        return false;
    },
    TimeSpan.FromMilliseconds(100));
