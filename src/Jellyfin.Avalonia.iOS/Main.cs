using UIKit;

namespace Jellyfin.Avalonia.iOS;

/// <summary>
/// The iOS entry point.
/// </summary>
internal static class Application
{
    private static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
