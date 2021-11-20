#pragma warning disable CA1711

using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Jellyfin.Maui;

/// <summary>
/// Maui application delegate.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <inheridoc />
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
