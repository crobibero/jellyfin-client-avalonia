using Jellyfin.Avalonia.Android.Profile;
using Jellyfin.Avalonia.LibVlc;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;
using Microsoft.Extensions.Logging;
using AndroidApplication = Android.App.Application;

namespace Jellyfin.Avalonia.Android;

/// <summary>
/// libVLC playback with capabilities detected from the Android media codecs.
/// </summary>
internal sealed class AndroidPlayerBackend : LibVlcPlayerBackend
{
    private readonly ILogger<AndroidPlayerBackend> _logger;
    private readonly Dictionary<bool, MediaCapabilities> _cache = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AndroidPlayerBackend"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public AndroidPlayerBackend(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AndroidPlayerBackend>();
    }

    /// <inheritdoc />
    public override string Name => "libVLC (MediaCodec)";

    /// <inheritdoc />
    public override MediaCapabilities GetCapabilities(PlaybackPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        lock (_cache)
        {
            if (!_cache.TryGetValue(preferences.SoftwareDecodingEnabled, out var capabilities))
            {
                try
                {
                    capabilities = AndroidMediaCapabilities.Detect(AndroidApplication.Context, preferences.SoftwareDecodingEnabled, base.GetCapabilities(preferences));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to query media codecs, using libVLC defaults");
                    capabilities = base.GetCapabilities(preferences);
                }

                _cache[preferences.SoftwareDecodingEnabled] = capabilities;
            }

            return capabilities;
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<string> GetOptions(PlaybackPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        foreach (var option in base.GetOptions(preferences))
        {
            if (!option.StartsWith("--avcodec-hw", StringComparison.Ordinal))
            {
                yield return option;
            }
        }

        // MediaCodec decoding on Android, software decoding only when allowed.
        yield return preferences.HardwareDecodingEnabled ? "--codec=mediacodec_ndk,mediacodec_jni,all" : "--codec=all";
    }
}
