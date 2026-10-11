namespace Jellyfin.Mvvm.Playback;

/// <summary>
/// The state of a media player.
/// </summary>
public enum PlayerState
{
    /// <summary>
    /// Nothing is loaded.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// Media is being opened.
    /// </summary>
    Opening = 1,

    /// <summary>
    /// Media is buffering.
    /// </summary>
    Buffering = 2,

    /// <summary>
    /// Media is playing.
    /// </summary>
    Playing = 3,

    /// <summary>
    /// Media is paused.
    /// </summary>
    Paused = 4,

    /// <summary>
    /// Media reached the end.
    /// </summary>
    Ended = 5,

    /// <summary>
    /// Playback failed.
    /// </summary>
    Error = 6
}
