using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Platform specific default preferences.
/// </summary>
public interface IPlatformDefaults
{
    /// <summary>
    /// Gets the default playback preferences.
    /// </summary>
    PlaybackPreferences Playback { get; }

    /// <summary>
    /// Gets the default user interface preferences.
    /// </summary>
    UserInterfacePreferences UserInterface { get; }
}
