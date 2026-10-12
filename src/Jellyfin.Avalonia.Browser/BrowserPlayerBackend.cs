using System.Runtime.Versioning;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// HTML5 video playback, with capabilities probed from the browser like the Jellyfin web client.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class BrowserPlayerBackend : IPlayerBackend
{
    private MediaCapabilities? _capabilities;

    /// <inheritdoc />
    public string Name => "HTML5 video";

    /// <inheritdoc />
    public MediaCapabilities GetCapabilities(PlaybackPreferences preferences)
        => _capabilities ??= Probe();

    /// <inheritdoc />
    public IMediaPlayer CreatePlayer(PlaybackPreferences preferences) => new BrowserMediaPlayer();

    private static bool CanPlay(string type) => !string.IsNullOrEmpty(BrowserInterop.CanPlayType(type));

    private static bool CanPlayOrMse(string type) => CanPlay(type) || BrowserInterop.IsTypeSupported(type);

    private static MediaCapabilities Probe()
    {
        var avc = CanPlayOrMse("video/mp4; codecs=\"avc1.42E01E\"");
        var avcLevel = CanPlayOrMse("video/mp4; codecs=\"avc1.640033\"") ? 51
            : CanPlayOrMse("video/mp4; codecs=\"avc1.640029\"") ? 41
            : 40;
        var hevc = CanPlayOrMse("video/mp4; codecs=\"hvc1.1.4.L123\"") || CanPlayOrMse("video/mp4; codecs=\"hev1.1.4.L123\"");
        var hevcMain10 = hevc && (CanPlayOrMse("video/mp4; codecs=\"hvc1.2.4.L153\"") || CanPlayOrMse("video/mp4; codecs=\"hev1.2.4.L153\""));
        var av1 = CanPlayOrMse("video/mp4; codecs=\"av01.0.15M.08\"");
        var av1Main10 = av1 && CanPlayOrMse("video/mp4; codecs=\"av01.0.15M.10\"");
        var vp9 = CanPlayOrMse("video/webm; codecs=\"vp9\"");
        var vp8 = CanPlayOrMse("video/webm; codecs=\"vp8\"");
        var hdrDisplay = BrowserInterop.Matches("(dynamic-range: high)");

        var videoCodecs = new List<string>();
        if (avc)
        {
            videoCodecs.Add(Codec.Video.H264);
        }

        if (hevc)
        {
            videoCodecs.Add(Codec.Video.Hevc);
        }

        if (av1)
        {
            videoCodecs.Add(Codec.Video.Av1);
        }

        if (vp9)
        {
            videoCodecs.Add(Codec.Video.Vp9);
        }

        if (vp8)
        {
            videoCodecs.Add(Codec.Video.Vp8);
        }

        var containers = new List<string> { Codec.Container.Mp4, Codec.Container.M4v };
        if (vp8 || vp9 || av1)
        {
            containers.Add(Codec.Container.WebM);
        }

        if (CanPlay("video/quicktime"))
        {
            containers.Add(Codec.Container.Mov);
        }

        var audioCodecs = new List<string> { Codec.Audio.Aac, Codec.Audio.Mp3 };
        if (CanPlayOrMse("audio/mp4; codecs=\"opus\"") || CanPlay("audio/ogg; codecs=\"opus\""))
        {
            audioCodecs.Add(Codec.Audio.Opus);
        }

        if (CanPlay("audio/flac") || CanPlayOrMse("audio/mp4; codecs=\"flac\""))
        {
            audioCodecs.Add(Codec.Audio.Flac);
        }

        if (CanPlay("audio/ogg; codecs=\"vorbis\""))
        {
            audioCodecs.Add(Codec.Audio.Vorbis);
        }

        var ac3 = CanPlayOrMse("audio/mp4; codecs=\"ac-3\"");
        if (ac3)
        {
            audioCodecs.Add(Codec.Audio.Ac3);
        }

        if (CanPlayOrMse("audio/mp4; codecs=\"ec-3\""))
        {
            audioCodecs.Add(Codec.Audio.Eac3);
        }

        if (CanPlay("audio/mp4; codecs=\"alac\""))
        {
            audioCodecs.Add(Codec.Audio.Alac);
        }

        return new MediaCapabilities
        {
            Name = AppConstants.Name + " Web",
            SupportsAvc = avc,
            AvcMainLevel = avcLevel,
            AvcMaxResolution = VideoResolution.Uhd4K,
            SupportsHevc = hevc,
            SupportsHevcMain10 = hevcMain10,
            HevcMainLevel = 153,
            HevcMain10Level = 153,
            HevcMaxResolution = VideoResolution.Uhd4K,
            SupportsHevcHdr10 = hevcMain10 && hdrDisplay,
            SupportsAv1 = av1,
            SupportsAv1Main10 = av1Main10,
            SupportsAv1Hdr10 = av1Main10 && hdrDisplay,
            Av1MaxResolution = VideoResolution.Uhd4K,
            DirectPlayVideoCodecs = videoCodecs,
            DirectPlayVideoContainers = containers,
            DirectPlayAudioContainers = [Codec.Container.Mp3, Codec.Audio.Aac, Codec.Audio.Flac, Codec.Container.WebM, Codec.Audio.Ogg, Codec.Audio.Oga, Codec.Container.Mp4, "m4a", Codec.Audio.Opus, Codec.Audio.Wav],
            DecodableAudioCodecs = audioCodecs,
            MaxAudioChannels = ac3 ? 6 : 2,
            HlsContainers = [Codec.Container.Mp4, Codec.Container.Ts],
            SupportsEmbeddedSubtitles = false,
            SupportsExternalSubtitles = true,
            ExternalTextSubtitleFormats = [Codec.Subtitle.Vtt],
            SupportsAssSubtitles = BrowserInterop.SupportsAssRenderer(),
            SupportsPgsSubtitles = true,
            SupportsExternalPgsSubtitles = true,
            SupportsBitmapSubtitles = false,
        };
    }
}
