namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A request to start playback.
/// </summary>
/// <param name="Items">The items to play. Folders such as series, seasons and albums are expanded.</param>
/// <param name="StartIndex">The index of the first item to play.</param>
/// <param name="StartPositionTicks">The position of the first item to start at, or <c>null</c> to start at the beginning.</param>
/// <param name="Shuffle">Whether to shuffle the items.</param>
public sealed record PlaybackRequest(
    IReadOnlyList<BaseItemDto> Items,
    int StartIndex,
    long? StartPositionTicks,
    bool Shuffle = false)
{
    /// <summary>
    /// Gets the media source (version) of the first item.
    /// </summary>
    public string? MediaSourceId { get; init; }

    /// <summary>
    /// Gets the audio stream index of the first item.
    /// </summary>
    public int? AudioStreamIndex { get; init; }

    /// <summary>
    /// Gets the subtitle stream index of the first item, -1 to disable subtitles.
    /// </summary>
    public int? SubtitleStreamIndex { get; init; }
}
