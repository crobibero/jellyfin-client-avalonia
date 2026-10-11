namespace Jellyfin.Mvvm.Playback;

/// <summary>
/// A request to load media in a <see cref="IMediaPlayer"/>.
/// </summary>
public sealed record MediaLoadRequest
{
    /// <summary>
    /// Gets the stream url.
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>
    /// Gets a value indicating whether the media contains video.
    /// </summary>
    public bool IsVideo { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the stream is an HLS transcode.
    /// </summary>
    public bool IsHls { get; init; }

    /// <summary>
    /// Gets the position to start playback at.
    /// </summary>
    public TimeSpan StartPosition { get; init; }

    /// <summary>
    /// Gets the known duration of the media, if any.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Gets the zero-based position of the audio track to select among the embedded audio tracks, if any.
    /// </summary>
    public int? AudioTrackPosition { get; init; }

    /// <summary>
    /// Gets the zero-based position of the subtitle track to select among the embedded subtitle tracks, if any.
    /// </summary>
    public int? SubtitleTrackPosition { get; init; }

    /// <summary>
    /// Gets the external subtitle to load and select, if any.
    /// </summary>
    public ExternalSubtitle? ExternalSubtitle { get; init; }

    /// <summary>
    /// Gets the display title.
    /// </summary>
    public string? Title { get; init; }
}

/// <summary>
/// An external subtitle file.
/// </summary>
/// <param name="Url">The subtitle url.</param>
/// <param name="Format">The subtitle format.</param>
/// <param name="Language">The subtitle language.</param>
public sealed record ExternalSubtitle(Uri Url, string Format, string? Language);
