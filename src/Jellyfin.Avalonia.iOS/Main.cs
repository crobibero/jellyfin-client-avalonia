using UIKit;

namespace Jellyfin.Avalonia.iOS;

/// <summary>
/// The iOS entry point.
/// </summary>
internal static class Application
{
    /// <summary>
    /// The main entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
