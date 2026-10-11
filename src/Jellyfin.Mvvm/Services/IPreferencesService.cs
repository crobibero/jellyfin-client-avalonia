using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Stores device preferences.
/// </summary>
public interface IPreferencesService
{
    /// <summary>
    /// Raised when preferences change.
    /// </summary>
    event EventHandler? PreferencesChanged;

    /// <summary>
    /// Gets the playback preferences.
    /// </summary>
    PlaybackPreferences Playback { get; }

    /// <summary>
    /// Gets the user interface preferences.
    /// </summary>
    UserInterfacePreferences UserInterface { get; }

    /// <summary>
    /// Load the preferences from storage.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask LoadAsync();

    /// <summary>
    /// Update and save the playback preferences.
    /// </summary>
    /// <param name="preferences">The preferences.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask SetPlaybackAsync(PlaybackPreferences preferences);

    /// <summary>
    /// Update and save the user interface preferences.
    /// </summary>
    /// <param name="preferences">The preferences.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask SetUserInterfaceAsync(UserInterfacePreferences preferences);
}
