using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Avalonia.LibVlc;

/// <summary>
/// A playback backend using libVLC.
/// </summary>
/// <remarks>
/// libVLC decodes nearly every codec in software and uses hardware decoders where available,
/// so the default capabilities are broad. Video is rendered through software frames which can't
/// be tone mapped, so HDR formats are reported unsupported and tone mapped by the server unless
/// the user overrides it. Platforms can override <see cref="GetCapabilities"/> with real decoder queries.
/// </remarks>
public class LibVlcPlayerBackend : IPlayerBackend, IDisposable
{
    private static readonly Lock _initializeLock = new();
    private static bool _initialized;

    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<LibVlcPlayerBackend> _logger;
    private readonly Lock _sync = new();
    private LibVLC? _libVlc;
    private string? _libVlcOptionsKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibVlcPlayerBackend"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public LibVlcPlayerBackend(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<LibVlcPlayerBackend>();
    }

    /// <inheritdoc />
    public virtual string Name => "libVLC";

    /// <summary>
    /// Gets the containers libVLC can play directly.
    /// </summary>
    protected static IReadOnlyList<string> Containers { get; } =
    [
        Codec.Container.Asf,
        Codec.Container.Avi,
        Codec.Container.Flv,
        Codec.Container.Hls,
        Codec.Container.M2ts,
        Codec.Container.M4v,
        Codec.Container.Mkv,
        Codec.Container.Mov,
        Codec.Container.Mp4,
        Codec.Container.Mpeg,
        Codec.Container.MpegTs,
        Codec.Container.Mpg,
        Codec.Container.Ogm,
        Codec.Container.Ogv,
        Codec.Container.ThreeGp,
        Codec.Container.Ts,
        Codec.Container.Vob,
        Codec.Container.WebM,
        Codec.Container.Wmv,
        Codec.Container.Wtv,
        Codec.Container.Xvid,
    ];

    /// <summary>
    /// Gets the video codecs libVLC can decode.
    /// </summary>
    protected static IReadOnlyList<string> VideoCodecs { get; } =
    [
        Codec.Video.Av1,
        Codec.Video.H264,
        Codec.Video.Hevc,
        Codec.Video.Mpeg,
        Codec.Video.Mpeg2Video,
        Codec.Video.Mpeg4,
        Codec.Video.Vc1,
        Codec.Video.Vp8,
        Codec.Video.Vp9,
    ];

    /// <summary>
    /// Gets the audio codecs libVLC can decode.
    /// </summary>
    protected static IReadOnlyList<string> AudioCodecs { get; } =
    [
        Codec.Audio.Aac,
        Codec.Audio.AacLatm,
        Codec.Audio.Ac3,
        Codec.Audio.Alac,
        Codec.Audio.Ape,
        Codec.Audio.Dca,
        Codec.Audio.Dts,
        Codec.Audio.Eac3,
        Codec.Audio.Flac,
        Codec.Audio.Mlp,
        Codec.Audio.Mp2,
        Codec.Audio.Mp3,
        Codec.Audio.Opus,
        Codec.Audio.PcmAlaw,
        Codec.Audio.PcmMulaw,
        Codec.Audio.PcmS16Le,
        Codec.Audio.PcmS20Le,
        Codec.Audio.PcmS24Le,
        Codec.Audio.TrueHd,
        Codec.Audio.Vorbis,
        Codec.Audio.Wma,
        Codec.Audio.Wmav2,
    ];

    /// <inheritdoc />
    public virtual MediaCapabilities GetCapabilities(PlaybackPreferences preferences)
        => new()
        {
            Name = AppConstants.Name + " libVLC",
            SupportsAvc = true,
            SupportsAvcHigh10 = true,
            AvcMainLevel = 62,
            AvcHigh10Level = 62,
            AvcMaxResolution = VideoResolution.Uhd4K,
            SupportsHevc = true,
            SupportsHevcMain10 = true,
            HevcMainLevel = 186,
            HevcMain10Level = 186,
            HevcMaxResolution = VideoResolution.Uhd4K,
            SupportsAv1 = true,
            SupportsAv1Main10 = true,
            Av1MaxResolution = VideoResolution.Uhd4K,
            SupportsVc1 = true,
            Vc1MaxResolution = VideoResolution.Uhd4K,
            DirectPlayVideoCodecs = VideoCodecs,
            DirectPlayVideoContainers = Containers,
            DecodableAudioCodecs = AudioCodecs,
            PassthroughAudioCodecs = [Codec.Audio.Ac3, Codec.Audio.Eac3, Codec.Audio.Dts, Codec.Audio.TrueHd],
            MaxAudioChannels = 8,
            HlsContainers = [Codec.Container.Ts, Codec.Container.Mp4],
            SupportsAssSubtitles = true,
            SupportsPgsSubtitles = true,
            SupportsBitmapSubtitles = true,
        };

    /// <inheritdoc />
    public IMediaPlayer CreatePlayer(PlaybackPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        return new LibVlcMediaPlayer(GetLibVlc(preferences), _loggerFactory.CreateLogger<LibVlcMediaPlayer>(), preferences.HardwareDecodingEnabled);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initialize the native libVLC library.
    /// </summary>
    protected virtual void InitializeCore() => Core.Initialize();

    /// <summary>
    /// Gets the libVLC options for the preferences.
    /// </summary>
    /// <param name="preferences">The playback preferences.</param>
    /// <returns>The options.</returns>
    protected virtual IEnumerable<string> GetOptions(PlaybackPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        yield return "--no-video-title-show";
        yield return "--no-snapshot-preview";
        yield return "--network-caching=1500";
        yield return "--avcodec-hw=" + (preferences.HardwareDecodingEnabled ? "any" : "none");
        if (preferences.AudioPassthrough)
        {
            yield return "--spdif";
        }

        if (preferences.AudioBehavior == AudioBehavior.DownmixToStereo)
        {
            yield return "--stereo-mode=1";
        }
    }

    /// <summary>
    /// Dispose the backend.
    /// </summary>
    /// <param name="disposing">Whether managed resources are disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _libVlc?.Dispose();
            _libVlc = null;
        }
    }

    private LibVLC GetLibVlc(PlaybackPreferences preferences)
    {
        lock (_initializeLock)
        {
            if (!_initialized)
            {
                InitializeCore();
                _initialized = true;
            }
        }

        var options = GetOptions(preferences).ToArray();
        var key = string.Join(' ', options);

        lock (_sync)
        {
            if (_libVlc is null || !string.Equals(key, _libVlcOptionsKey, StringComparison.Ordinal))
            {
                // Players created with the previous instance keep it alive through their own references.
                _logger.LogInformation("Creating libVLC with options {Options}", key);
                _libVlc = new LibVLC(options);
                _libVlcOptionsKey = key;
            }

            return _libVlc;
        }
    }
}
