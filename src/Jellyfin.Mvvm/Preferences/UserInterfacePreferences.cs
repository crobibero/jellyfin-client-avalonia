namespace Jellyfin.Mvvm.Preferences;

/// <summary>
/// Preferences for the user interface.
/// </summary>
public sealed record UserInterfacePreferences
{
    /// <summary>
    /// Gets a value indicating whether item backdrops are shown behind the content.
    /// </summary>
    public bool BackdropEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the clock is shown in the toolbar.
    /// </summary>
    public bool ClockEnabled { get; init; } = true;

    /// <summary>
    /// Gets the watched indicator behavior.
    /// </summary>
    public WatchedIndicatorBehavior WatchedIndicatorBehavior { get; init; } = WatchedIndicatorBehavior.Always;

    /// <summary>
    /// Gets a value indicating whether theme music is played on item details.
    /// </summary>
    public bool ThemeMusicEnabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether series thumbnails are used for episodes in home rows.
    /// </summary>
    public bool UseSeriesThumbnails { get; init; } = true;
}
