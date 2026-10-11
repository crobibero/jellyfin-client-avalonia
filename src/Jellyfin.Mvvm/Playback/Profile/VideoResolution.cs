namespace Jellyfin.Mvvm.Playback.Profile;

/// <summary>
/// A maximum decodable video resolution.
/// </summary>
/// <param name="Width">The width in pixels.</param>
/// <param name="Height">The height in pixels.</param>
public readonly record struct VideoResolution(int Width, int Height)
{
    /// <summary>
    /// Gets the 4K DCI resolution, used as a default for capable software decoders.
    /// </summary>
    public static VideoResolution Uhd4K { get; } = new(4096, 2304);

    /// <summary>
    /// Gets the 8K resolution.
    /// </summary>
    public static VideoResolution Uhd8K { get; } = new(8192, 4320);

    /// <summary>
    /// Gets the 1080p resolution.
    /// </summary>
    public static VideoResolution FullHd { get; } = new(1920, 1080);
}
