namespace Jellyfin.Mvvm.Playback;

/// <summary>
/// A platform media player.
/// </summary>
/// <remarks>
/// Events may be raised on any thread.
/// </remarks>
public interface IMediaPlayer : IDisposable
{
    /// <summary>
    /// Raised when <see cref="State"/> changes.
    /// </summary>
    event EventHandler? StateChanged;

    /// <summary>
    /// Raised when the playback position changes.
    /// </summary>
    event EventHandler? PositionChanged;

    /// <summary>
    /// Raised when an error occurs.
    /// </summary>
    event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Gets the player state.
    /// </summary>
    PlayerState State { get; }

    /// <summary>
    /// Gets the playback position.
    /// </summary>
    TimeSpan Position { get; }

    /// <summary>
    /// Gets the media duration.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets a value indicating whether the media is seekable.
    /// </summary>
    bool CanSeek { get; }

    /// <summary>
    /// Gets or sets the volume, from 0 to 100.
    /// </summary>
    int Volume { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether audio is muted.
    /// </summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Gets or sets the playback speed.
    /// </summary>
    float Speed { get; set; }

    /// <summary>
    /// Load media and start playback.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask LoadAsync(MediaLoadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume playback.
    /// </summary>
    void Play();

    /// <summary>
    /// Pause playback.
    /// </summary>
    void Pause();

    /// <summary>
    /// Stop playback and unload media.
    /// </summary>
    void Stop();

    /// <summary>
    /// Seek to a position.
    /// </summary>
    /// <param name="position">The position.</param>
    void Seek(TimeSpan position);

    /// <summary>
    /// Select an embedded audio track.
    /// </summary>
    /// <param name="position">The zero-based position among embedded audio tracks.</param>
    void SelectAudioTrack(int position);

    /// <summary>
    /// Select an embedded subtitle track.
    /// </summary>
    /// <param name="position">The zero-based position among embedded subtitle tracks, or <c>null</c> to disable subtitles.</param>
    void SelectSubtitleTrack(int? position);

    /// <summary>
    /// Load and select an external subtitle.
    /// </summary>
    /// <param name="subtitle">The subtitle.</param>
    void LoadExternalSubtitle(ExternalSubtitle subtitle);
}
