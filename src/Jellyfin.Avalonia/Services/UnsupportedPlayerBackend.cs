using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// A backend used when the platform has no player.
/// </summary>
public class UnsupportedPlayerBackend : IPlayerBackend
{
    /// <inheritdoc />
    public string Name => "None";

    /// <inheritdoc />
    public MediaCapabilities GetCapabilities(PlaybackPreferences preferences) => new()
    {
        Name = AppConstants.Name,
        SupportsAvc = true,
        AvcMainLevel = 41,
        AvcMaxResolution = VideoResolution.FullHd,
        DirectPlayVideoCodecs = [Codec.Video.H264],
        DirectPlayVideoContainers = [Codec.Container.Mp4],
        DecodableAudioCodecs = [Codec.Audio.Aac, Codec.Audio.Mp3],
    };

    /// <inheritdoc />
    public IMediaPlayer CreatePlayer(PlaybackPreferences preferences)
        => throw new PlatformNotSupportedException(Mvvm.Strings.Strings.Player_Unsupported);
}
