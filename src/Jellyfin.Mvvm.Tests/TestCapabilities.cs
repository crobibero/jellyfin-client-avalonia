using Jellyfin.Mvvm.Playback.Profile;

namespace Jellyfin.Mvvm.Tests;

internal static class TestCapabilities
{
    public static MediaCapabilities Full { get; } = new()
    {
        Name = "Test",
        SupportsAvc = true,
        SupportsAvcHigh10 = true,
        AvcMainLevel = 62,
        AvcHigh10Level = 62,
        AvcMaxResolution = VideoResolution.Uhd8K,
        SupportsHevc = true,
        SupportsHevcMain10 = true,
        HevcMainLevel = 186,
        HevcMain10Level = 186,
        HevcMaxResolution = VideoResolution.Uhd8K,
        SupportsHevcHdr10 = true,
        SupportsHevcHdr10Plus = true,
        SupportsHevcDolbyVision = true,
        SupportsHevcDolbyVisionEl = true,
        SupportsAv1 = true,
        SupportsAv1Main10 = true,
        SupportsAv1Hdr10 = true,
        SupportsAv1Hdr10Plus = true,
        SupportsAv1DolbyVision = true,
        Av1MaxResolution = VideoResolution.Uhd8K,
        SupportsVc1 = true,
        Vc1MaxResolution = VideoResolution.Uhd4K,
        DirectPlayVideoCodecs = [Codec.Video.H264, Codec.Video.Hevc, Codec.Video.Av1, Codec.Video.Vc1, Codec.Video.Vp9],
        DirectPlayVideoContainers = [Codec.Container.Mkv, Codec.Container.Mp4],
        DecodableAudioCodecs = [Codec.Audio.Aac, Codec.Audio.Ac3, Codec.Audio.Eac3, Codec.Audio.Dts, Codec.Audio.TrueHd, Codec.Audio.Flac, Codec.Audio.Mp3, Codec.Audio.Opus],
        SupportsAssSubtitles = true,
        SupportsPgsSubtitles = true,
    };

    public static MediaCapabilities SdrOnlyH264 { get; } = Full with
    {
        SupportsAvcHigh10 = false,
        AvcMainLevel = 41,
        AvcMaxResolution = VideoResolution.FullHd,
        SupportsHevc = false,
        SupportsHevcMain10 = false,
        SupportsHevcHdr10 = false,
        SupportsHevcHdr10Plus = false,
        SupportsHevcDolbyVision = false,
        SupportsHevcDolbyVisionEl = false,
        SupportsAv1 = false,
        SupportsAv1Main10 = false,
        SupportsAv1Hdr10 = false,
        SupportsAv1Hdr10Plus = false,
        SupportsAv1DolbyVision = false,
        SupportsVc1 = false,
        DecodableAudioCodecs = [Codec.Audio.Aac, Codec.Audio.Mp3],
        PassthroughAudioCodecs = [Codec.Audio.Ac3, Codec.Audio.Eac3],
    };
}
