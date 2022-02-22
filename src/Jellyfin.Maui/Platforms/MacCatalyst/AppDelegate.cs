#pragma warning disable CA1711


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
