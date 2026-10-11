using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Mvvm.Playback;

/// <summary>
/// A platform playback backend.
/// </summary>
public interface IPlayerBackend
{
    /// <summary>
    /// Gets the backend name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the media capabilities of the device using this backend.
    /// </summary>
    /// <param name="preferences">The playback preferences.</param>
    /// <returns>The capabilities.</returns>
    MediaCapabilities GetCapabilities(PlaybackPreferences preferences);

    /// <summary>
    /// Create a media player.
    /// </summary>
    /// <param name="preferences">The playback preferences.</param>
    /// <returns>The media player.</returns>
    IMediaPlayer CreatePlayer(PlaybackPreferences preferences);
}
