using Avalonia.Controls;
using Avalonia.Media;
using Jellyfin.Mvvm.Playback;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Creates the control rendering a player's video.
/// </summary>
public interface IVideoViewFactory
{
    /// <summary>
    /// Gets a value indicating whether video is rendered behind the Avalonia content, which must then be transparent.
    /// </summary>
    bool RendersBehindContent => false;

    /// <summary>
    /// Create the video view for a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The video view, or <c>null</c> when the player renders outside Avalonia.</returns>
    Control? CreateVideoView(IMediaPlayer player);
}

/// <summary>
/// A control that renders video.
/// </summary>
public interface IVideoView
{
    /// <summary>
    /// Gets or sets how the video is scaled.
    /// </summary>
    Stretch VideoStretch { get; set; }
}

/// <summary>
/// A video view factory for platforms without video playback.
/// </summary>
public class NullVideoViewFactory : IVideoViewFactory
{
    /// <inheritdoc />
    public Control? CreateVideoView(IMediaPlayer player) => null;
}
