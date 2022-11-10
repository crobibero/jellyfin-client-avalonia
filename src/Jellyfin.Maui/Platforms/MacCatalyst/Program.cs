using UIKit;

namespace Jellyfin.Maui;

/// <summary>
/// The program entrypoint.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entrypoint for the application.
    /// </summary>
    /// <param name="args">The runtime arguments.</param>
    public static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
