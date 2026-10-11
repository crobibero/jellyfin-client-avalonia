namespace Jellyfin.Avalonia;

/// <summary>
/// Constants for use in the application.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// The app name.
    /// </summary>
    public const string Name = "Jellyfin Avalonia";

    /// <summary>
    /// The app identifier used for storage paths.
    /// </summary>
    public const string Identifier = "jellyfin-avalonia";

    /// <summary>
    /// Gets the app version.
    /// </summary>
    public static string Version { get; } = typeof(AppConstants).Assembly.GetName().Version?.ToString(3) ?? "0.1.0";
}
