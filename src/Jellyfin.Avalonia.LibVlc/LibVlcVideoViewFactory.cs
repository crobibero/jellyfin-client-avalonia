using Avalonia.Controls;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;

namespace Jellyfin.Avalonia.LibVlc;

/// <summary>
/// Creates video views for libVLC players.
/// </summary>
public class LibVlcVideoViewFactory : IVideoViewFactory
{
    /// <inheritdoc />
    public Control? CreateVideoView(IMediaPlayer player)
        => player is LibVlcMediaPlayer vlcPlayer ? new LibVlcVideoView(vlcPlayer.Frames) : null;
}
