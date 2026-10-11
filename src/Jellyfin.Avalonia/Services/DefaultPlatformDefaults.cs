using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Default preferences.
/// </summary>
public class DefaultPlatformDefaults : IPlatformDefaults
{
    /// <inheritdoc />
    public virtual PlaybackPreferences Playback { get; } = new();

    /// <inheritdoc />
    public virtual UserInterfacePreferences UserInterface { get; } = new();
}
