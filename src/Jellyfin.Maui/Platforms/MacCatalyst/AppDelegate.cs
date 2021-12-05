#pragma warning disable CA1711

using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Jellyfin.Maui.Platforms.MacCatalyst;

/// <summary>
/// The macOS app delegate.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
