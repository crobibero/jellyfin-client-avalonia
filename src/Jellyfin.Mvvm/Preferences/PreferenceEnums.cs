#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name

namespace Jellyfin.Mvvm.Preferences;

/// <summary>
/// How audio is delivered to the player.
/// </summary>
public enum AudioBehavior
{
    /// <summary>
    /// Directly stream audio without any changes.
    /// </summary>
    DirectStream = 0,

    /// <summary>
    /// Downmix audio to stereo.
    /// </summary>
    DownmixToStereo = 1
}

/// <summary>
/// How a bitstream audio format should be treated in the device profile.
/// </summary>
public enum BitstreamAudioMode
{
    /// <summary>
    /// Use the detected device capabilities.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Always allow the format.
    /// </summary>
    Enable = 1,

    /// <summary>
    /// Never allow the format, the server will transcode it.
    /// </summary>
    Disable = 2
}

/// <summary>
/// How an HDR format should be treated in the device profile, overriding device-reported capabilities.
/// </summary>
public enum HdrOverrideMode
{
    /// <summary>
    /// Use the detected device capabilities.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Always allow the format.
    /// </summary>
    Enable = 1,

    /// <summary>
    /// Never allow the format, the server will tone map it.
    /// </summary>
    Disable = 2
}

/// <summary>
/// The action to take when playback reaches a media segment.
/// </summary>
public enum MediaSegmentAction
{
    /// <summary>
    /// Don't take any action.
    /// </summary>
    Nothing = 0,

    /// <summary>
    /// Skip to the end of the segment.
    /// </summary>
    Skip = 1,

    /// <summary>
    /// Ask the user whether to skip the segment.
    /// </summary>
    AskToSkip = 2
}

/// <summary>
/// The behavior of the next up screen at the end of an episode.
/// </summary>
public enum NextUpBehavior
{
    /// <summary>
    /// Show the next up prompt with the item's artwork.
    /// </summary>
    Extended = 0,

    /// <summary>
    /// Show a minimal next up prompt.
    /// </summary>
    Minimal = 1,

    /// <summary>
    /// Disable the next up prompt.
    /// </summary>
    Disabled = 2
}

/// <summary>
/// How the video is scaled in the player.
/// </summary>
public enum ZoomMode
{
    /// <summary>
    /// Fit the video inside the screen.
    /// </summary>
    Fit = 0,

    /// <summary>
    /// Fill the screen, cropping the video.
    /// </summary>
    AutoCrop = 1,

    /// <summary>
    /// Stretch the video to the screen.
    /// </summary>
    Stretch = 2
}

/// <summary>
/// When to show the watched indicator on cards.
/// </summary>
public enum WatchedIndicatorBehavior
{
    /// <summary>
    /// Always show the indicator.
    /// </summary>
    Always = 0,

    /// <summary>
    /// Only show the watched checkmark, hide unwatched counts.
    /// </summary>
    HideUnwatched = 1,

    /// <summary>
    /// Only show indicators on episodes.
    /// </summary>
    EpisodesOnly = 2,

    /// <summary>
    /// Never show the indicator.
    /// </summary>
    Never = 3
}
