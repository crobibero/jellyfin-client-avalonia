namespace Jellyfin.Avalonia;

/// <summary>
/// Constants for use in the application.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// The app name.
    /// </summary>
    public const string Name = "Jellyfin-Avalonia";

    /// <summary>
    /// The app http client name.
    /// </summary>
    public const string HttpClient = "Default";

    /// <summary>
    /// The app version.
    /// </summary>
    public static readonly string Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.1";
}
