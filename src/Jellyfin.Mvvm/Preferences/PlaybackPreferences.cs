namespace Jellyfin.Mvvm.Preferences;

/// <summary>
/// Playback preferences that influence the device profile and the player.
/// </summary>
public sealed record PlaybackPreferences
{
    /// <summary>
    /// The default maximum bitrate in megabits per second.
    /// </summary>
    public const double DefaultMaxBitrateMbps = 100;

    /// <summary>
    /// Gets the maximum streaming bitrate in megabits per second.
    /// </summary>
    public double MaxBitrateMbps { get; init; } = DefaultMaxBitrateMbps;

    /// <summary>
    /// Gets the audio behavior.
    /// </summary>
    public AudioBehavior AudioBehavior { get; init; } = AudioBehavior.DirectStream;

    /// <summary>
    /// Gets the AC-3 bitstream mode.
    /// </summary>
    public BitstreamAudioMode BitstreamAc3 { get; init; } = BitstreamAudioMode.Auto;

    /// <summary>
    /// Gets the E-AC-3 bitstream mode.
    /// </summary>
    public BitstreamAudioMode BitstreamEac3 { get; init; } = BitstreamAudioMode.Auto;

    /// <summary>
    /// Gets the DTS bitstream mode.
    /// </summary>
    public BitstreamAudioMode BitstreamDts { get; init; } = BitstreamAudioMode.Auto;

    /// <summary>
    /// Gets the TrueHD bitstream mode.
    /// </summary>
    public BitstreamAudioMode BitstreamTrueHd { get; init; } = BitstreamAudioMode.Auto;

    /// <summary>
    /// Gets a value indicating whether audio passthrough to a receiver should be used when available.
    /// </summary>
    public bool AudioPassthrough { get; init; }

    /// <summary>
    /// Gets a value indicating whether ASS/SSA subtitles are direct played (rendered by the player).
    /// </summary>
    public bool AssDirectPlay { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether PGS subtitles are direct played (rendered by the player).
    /// </summary>
    public bool PgsDirectPlay { get; init; } = true;

    /// <summary>
    /// Gets the user AVC level override, or <c>null</c> for automatic detection.
    /// </summary>
    public int? AvcLevel { get; init; }

    /// <summary>
    /// Gets the user HEVC level override, or <c>null</c> for automatic detection.
    /// </summary>
    public int? HevcLevel { get; init; }

    /// <summary>
    /// Gets the HDR10 override.
    /// </summary>
    public HdrOverrideMode Hdr10Override { get; init; } = HdrOverrideMode.Auto;

    /// <summary>
    /// Gets the HDR10+ override.
    /// </summary>
    public HdrOverrideMode Hdr10PlusOverride { get; init; } = HdrOverrideMode.Auto;

    /// <summary>
    /// Gets the HLG override.
    /// </summary>
    public HdrOverrideMode HlgOverride { get; init; } = HdrOverrideMode.Auto;

    /// <summary>
    /// Gets the Dolby Vision profile 5 override.
    /// </summary>
    public HdrOverrideMode DoviProfile5Override { get; init; } = HdrOverrideMode.Auto;

    /// <summary>
    /// Gets the Dolby Vision profile 7 override.
    /// </summary>
    public HdrOverrideMode DoviProfile7Override { get; init; } = HdrOverrideMode.Auto;

    /// <summary>
    /// Gets the Dolby Vision profile 8 override.
    /// </summary>
    public HdrOverrideMode DoviProfile8Override { get; init; } = HdrOverrideMode.Auto;

    /// <summary>
    /// Gets a value indicating whether software decoders may be used when detecting capabilities.
    /// </summary>
    public bool SoftwareDecodingEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether hardware accelerated decoding should be used by the player.
    /// </summary>
    public bool HardwareDecodingEnabled { get; init; } = true;

    /// <summary>
    /// Gets the number of seconds to rewind when resuming playback.
    /// </summary>
    public int ResumeRewindSeconds { get; init; }

    /// <summary>
    /// Gets the skip back length in seconds.
    /// </summary>
    public int SkipBackSeconds { get; init; } = 10;

    /// <summary>
    /// Gets the skip forward length in seconds.
    /// </summary>
    public int SkipForwardSeconds { get; init; } = 30;

    /// <summary>
    /// Gets the next up behavior.
    /// </summary>
    public NextUpBehavior NextUpBehavior { get; init; } = NextUpBehavior.Extended;

    /// <summary>
    /// Gets the next up countdown in seconds, 0 to wait for the user.
    /// </summary>
    public int NextUpTimeoutSeconds { get; init; } = 7;

    /// <summary>
    /// Gets the action for intro segments.
    /// </summary>
    public MediaSegmentAction IntroAction { get; init; } = MediaSegmentAction.AskToSkip;

    /// <summary>
    /// Gets the action for outro segments.
    /// </summary>
    public MediaSegmentAction OutroAction { get; init; } = MediaSegmentAction.AskToSkip;

    /// <summary>
    /// Gets the action for recap segments.
    /// </summary>
    public MediaSegmentAction RecapAction { get; init; } = MediaSegmentAction.Nothing;

    /// <summary>
    /// Gets the action for preview segments.
    /// </summary>
    public MediaSegmentAction PreviewAction { get; init; } = MediaSegmentAction.Nothing;

    /// <summary>
    /// Gets the action for commercial segments.
    /// </summary>
    public MediaSegmentAction CommercialAction { get; init; } = MediaSegmentAction.Nothing;

    /// <summary>
    /// Gets the player zoom mode.
    /// </summary>
    public ZoomMode ZoomMode { get; init; } = ZoomMode.Fit;

    /// <summary>
    /// Gets the maximum bitrate in bits per second.
    /// </summary>
    /// <returns>The bitrate in bits per second.</returns>
    public int GetMaxBitrate()
    {
        var mbps = MaxBitrateMbps;

        // Guard against a 0 value preventing playback.
        if (mbps < 0.01)
        {
            mbps = DefaultMaxBitrateMbps;
        }

        return (int)Math.Round(mbps * 1_000_000);
    }
}
