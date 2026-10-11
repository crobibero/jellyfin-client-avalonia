using Jellyfin.Avalonia.LibVlc;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Avalonia.iOS;

/// <summary>
/// libVLC playback on Apple platforms, decoding H.264 and HEVC with VideoToolbox.
/// </summary>
internal sealed class AppleLibVlcBackend : LibVlcPlayerBackend
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppleLibVlcBackend"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public AppleLibVlcBackend(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
    }

    /// <inheritdoc />
    public override string Name => "libVLC (VideoToolbox)";

    /// <inheritdoc />
    public override MediaCapabilities GetCapabilities(PlaybackPreferences preferences)
        => base.GetCapabilities(preferences) with
        {
            Name = AppConstants.Name + " iOS",

            // Software decoding of high bitrate AV1 and VC-1 is too slow on most devices.
            SupportsAv1 = preferences?.SoftwareDecodingEnabled == true,
            SupportsAv1Main10 = preferences?.SoftwareDecodingEnabled == true,
            SupportsVc1 = preferences?.SoftwareDecodingEnabled == true,
        };
}
