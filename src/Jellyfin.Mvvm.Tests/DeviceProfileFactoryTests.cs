using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Mvvm.Tests;

public class DeviceProfileFactoryTests
{
    private static readonly PlaybackPreferences _defaults = new();

    private static CodecProfile? FindRangeTypeProfile(DeviceProfile profile, string codec)
        => profile.CodecProfiles!.FirstOrDefault(p => p.Codec == codec && p.Conditions!.Any(c => c.Property == ProfileCondition_Property.VideoRangeType));

    private static string[] GetRangeTypes(CodecProfile profile)
        => profile.Conditions!.Single(c => c.Property == ProfileCondition_Property.VideoRangeType).Value!.Split('|');

    [Fact]
    public void Create_SetsBitrateFromPreferences()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults with { MaxBitrateMbps = 20 });

        Assert.Equal(20_000_000, profile.MaxStreamingBitrate);
        Assert.Equal(20_000_000, profile.MaxStaticBitrate);
    }

    [Fact]
    public void Create_ZeroBitrateFallsBackToDefault()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults with { MaxBitrateMbps = 0 });

        Assert.Equal((int)(PlaybackPreferences.DefaultMaxBitrateMbps * 1_000_000), profile.MaxStreamingBitrate);
    }

    [Fact]
    public void Create_FullCapabilities_DirectPlaysEverything()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults);

        var video = Assert.Single(profile.DirectPlayProfiles!, p => p.Type == DirectPlayProfile_Type.Video);
        Assert.Contains("hevc", video.VideoCodec, StringComparison.Ordinal);
        Assert.Contains("truehd", video.AudioCodec, StringComparison.Ordinal);
        Assert.Equal("mkv,mp4", video.Container);

        Assert.DoesNotContain(profile.CodecProfiles!, p => p.Conditions!.Any(c => c.Condition == ProfileCondition_Condition.Equals && c.Value == "none"));
    }

    [Fact]
    public void Create_FullCapabilities_OnlyExcludesInvalidDolbyVision()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults);

        var hevc = FindRangeTypeProfile(profile, Codec.Video.Hevc);
        Assert.NotNull(hevc);
        Assert.Equal([VideoRangeTypes.DoviInvalid], GetRangeTypes(hevc));
    }

    [Fact]
    public void Create_NoHevc_DisablesHevcAndTranscodesToH264()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.SdrOnlyH264, _defaults);

        var hevc = profile.CodecProfiles!.First(p => p.Codec == Codec.Video.Hevc);
        var condition = Assert.Single(hevc.Conditions!);
        Assert.Equal(ProfileCondition_Property.VideoProfile, condition.Property);
        Assert.Equal(ProfileCondition_Condition.Equals, condition.Condition);
        Assert.Equal("none", condition.Value);

        Assert.All(profile.TranscodingProfiles!.Where(p => p.Type == TranscodingProfile_Type.Video), p => Assert.Equal("h264", p.VideoCodec));
    }

    [Fact]
    public void Create_H264Only_LimitsLevelAndResolution()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.SdrOnlyH264, _defaults);

        Assert.Contains(profile.CodecProfiles!, p => p.Codec == Codec.Video.H264
            && p.Conditions!.Any(c => c.Property == ProfileCondition_Property.VideoLevel && c.Value == "41"));
        Assert.Contains(profile.CodecProfiles!, p => p.Codec == Codec.Video.H264
            && p.Conditions!.Any(c => c.Property == ProfileCondition_Property.Width && c.Value == "1920")
            && p.Conditions!.Any(c => c.Property == ProfileCondition_Property.Height && c.Value == "1080"));

        var h264Profiles = profile.CodecProfiles!.First(p => p.Codec == Codec.Video.H264);
        Assert.DoesNotContain("high 10", h264Profiles.Conditions![0].Value, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_UserLevelOverride_ReplacesDetectedLevel()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.SdrOnlyH264, _defaults with { AvcLevel = 51 });

        Assert.Contains(profile.CodecProfiles!, p => p.Codec == Codec.Video.H264
            && p.Conditions!.Any(c => c.Property == ProfileCondition_Property.VideoLevel && c.Value == "51"));
    }

    [Fact]
    public void Create_NoHdrSupport_ExcludesHdrRangeTypes()
    {
        var capabilities = TestCapabilities.Full with
        {
            SupportsHevcHdr10 = false,
            SupportsHevcHdr10Plus = false,
            SupportsHevcDolbyVision = false,
            SupportsHevcDolbyVisionEl = false,
        };

        var profile = DeviceProfileFactory.Create(capabilities, _defaults);
        var rangeTypes = GetRangeTypes(FindRangeTypeProfile(profile, Codec.Video.Hevc)!);

        Assert.Contains(VideoRangeTypes.Hdr10, rangeTypes);
        Assert.Contains(VideoRangeTypes.Hdr10Plus, rangeTypes);
        Assert.Contains(VideoRangeTypes.Dovi, rangeTypes);
        Assert.Contains(VideoRangeTypes.DoviWithHdr10, rangeTypes);
        Assert.Contains(VideoRangeTypes.DoviWithEl, rangeTypes);
        Assert.DoesNotContain(VideoRangeTypes.Hlg, rangeTypes);
        Assert.DoesNotContain(VideoRangeTypes.Sdr, rangeTypes);
    }

    [Fact]
    public void Create_Hdr10WithoutDolbyVision_KeepsDolbyVisionWithHdr10Fallback()
    {
        var capabilities = TestCapabilities.Full with
        {
            SupportsHevcDolbyVision = false,
            SupportsHevcDolbyVisionEl = false,
        };

        var profile = DeviceProfileFactory.Create(capabilities, _defaults);
        var rangeTypes = GetRangeTypes(FindRangeTypeProfile(profile, Codec.Video.Hevc)!);

        Assert.Contains(VideoRangeTypes.Dovi, rangeTypes);
        Assert.DoesNotContain(VideoRangeTypes.DoviWithHdr10, rangeTypes);
        Assert.DoesNotContain(VideoRangeTypes.Hdr10, rangeTypes);
    }

    [Fact]
    public void Create_HdrOverrides_AreApplied()
    {
        var capabilities = TestCapabilities.Full with { SupportsHevcHdr10 = false, SupportsHevcHdr10Plus = false };
        var preferences = _defaults with { Hdr10Override = HdrOverrideMode.Enable, HlgOverride = HdrOverrideMode.Disable };

        var profile = DeviceProfileFactory.Create(capabilities, preferences);
        var rangeTypes = GetRangeTypes(FindRangeTypeProfile(profile, Codec.Video.Hevc)!);

        Assert.DoesNotContain(VideoRangeTypes.Hdr10, rangeTypes);
        Assert.Contains(VideoRangeTypes.Hdr10Plus, rangeTypes);
        Assert.Contains(VideoRangeTypes.Hlg, rangeTypes);
    }

    [Fact]
    public void Create_RangeTypeProfile_UsesApplyConditions()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.SdrOnlyH264, _defaults);
        var av1 = FindRangeTypeProfile(profile, Codec.Video.Av1)!;

        var condition = Assert.Single(av1.Conditions!);
        var apply = Assert.Single(av1.ApplyConditions!);
        Assert.Equal(ProfileCondition_Condition.NotEquals, condition.Condition);
        Assert.Equal(ProfileCondition_Condition.EqualsAny, apply.Condition);
        Assert.Equal(condition.Value, apply.Value);
    }

    [Fact]
    public void Create_Downmix_LimitsAudio()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults with { AudioBehavior = AudioBehavior.DownmixToStereo });

        var video = profile.DirectPlayProfiles!.First(p => p.Type == DirectPlayProfile_Type.Video);
        Assert.Equal("aac,mp3", video.AudioCodec);

        var channels = profile.CodecProfiles!.Single(p => p.Type == CodecProfile_Type.VideoAudio);
        Assert.Equal("2", channels.Conditions![0].Value);
    }

    [Fact]
    public void Create_BitstreamDisable_RemovesCodec()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults with { BitstreamAc3 = BitstreamAudioMode.Disable });

        var video = profile.DirectPlayProfiles!.First(p => p.Type == DirectPlayProfile_Type.Video);
        var codecs = video.AudioCodec!.Split(',');
        Assert.DoesNotContain("ac3", codecs);
        Assert.Contains("eac3", codecs);
    }

    [Fact]
    public void GetAllowedAudioCodecs_PassthroughOnlyCodecs_RequirePassthroughPreference()
    {
        var capabilities = TestCapabilities.SdrOnlyH264;

        var withoutPassthrough = DeviceProfileFactory.GetAllowedAudioCodecs(capabilities, _defaults, downMixAudio: false);
        var withPassthrough = DeviceProfileFactory.GetAllowedAudioCodecs(capabilities, _defaults with { AudioPassthrough = true }, downMixAudio: false);
        var forced = DeviceProfileFactory.GetAllowedAudioCodecs(capabilities, _defaults with { BitstreamEac3 = BitstreamAudioMode.Enable }, downMixAudio: false);

        Assert.DoesNotContain(Codec.Audio.Ac3, withoutPassthrough);
        Assert.Contains(Codec.Audio.Ac3, withPassthrough);
        Assert.Contains(Codec.Audio.Eac3, forced);
        Assert.DoesNotContain(Codec.Audio.Ac3, forced);
    }

    [Fact]
    public void Create_Subtitles_RespectCapabilitiesAndPreferences()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults with { PgsDirectPlay = false });

        Assert.Contains(profile.SubtitleProfiles!, p => p.Format == "ass" && p.Method == SubtitleProfile_Method.Embed);
        Assert.DoesNotContain(profile.SubtitleProfiles!, p => p.Format == "pgssub" && p.Method == SubtitleProfile_Method.Embed);
        Assert.Contains(profile.SubtitleProfiles!, p => p.Format == "pgssub" && p.Method == SubtitleProfile_Method.Encode);
        Assert.Contains(profile.SubtitleProfiles!, p => p.Format == "vtt" && p.Method == SubtitleProfile_Method.Hls);
    }

    [Fact]
    public void Create_BrowserLikeCapabilities_OnlyAllowExternalVtt()
    {
        var capabilities = TestCapabilities.SdrOnlyH264 with
        {
            SupportsEmbeddedSubtitles = false,
            SupportsAssSubtitles = false,
            SupportsPgsSubtitles = false,
            ExternalTextSubtitleFormats = [Codec.Subtitle.Vtt],
        };

        var profile = DeviceProfileFactory.Create(capabilities, _defaults);

        Assert.DoesNotContain(profile.SubtitleProfiles!, p => p.Method == SubtitleProfile_Method.Embed);
        var external = Assert.Single(profile.SubtitleProfiles!, p => p.Method == SubtitleProfile_Method.External);
        Assert.Equal("vtt", external.Format);
    }

    [Fact]
    public void Create_BrowserLikeCapabilities_LoadAssAndPgsExternally()
    {
        var capabilities = TestCapabilities.SdrOnlyH264 with
        {
            SupportsEmbeddedSubtitles = false,
            SupportsAssSubtitles = true,
            SupportsPgsSubtitles = true,
            SupportsExternalPgsSubtitles = true,
            ExternalTextSubtitleFormats = [Codec.Subtitle.Vtt],
        };

        var profile = DeviceProfileFactory.Create(capabilities, _defaults);
        var external = profile.SubtitleProfiles!.Where(p => p.Method == SubtitleProfile_Method.External).Select(p => p.Format).Order(StringComparer.Ordinal);
        Assert.Equal(["ass", "pgs", "pgssub", "ssa", "vtt"], external);

        profile = DeviceProfileFactory.Create(capabilities, _defaults with { PgsDirectPlay = false });
        Assert.DoesNotContain(profile.SubtitleProfiles!, p => p.Format == "pgssub" && p.Method == SubtitleProfile_Method.External);
        Assert.Contains(profile.SubtitleProfiles!, p => p.Format == "pgssub" && p.Method == SubtitleProfile_Method.Encode);
    }

    [Fact]
    public void Create_EmbeddedPgsOnly_DoesNotLoadPgsExternally()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full, _defaults);

        Assert.Contains(profile.SubtitleProfiles!, p => p.Format == "pgssub" && p.Method == SubtitleProfile_Method.Embed);
        Assert.DoesNotContain(profile.SubtitleProfiles!, p => p.Format == "pgssub" && p.Method == SubtitleProfile_Method.External);
    }

    [Fact]
    public void Create_TranscodingProfiles_FollowHlsContainers()
    {
        var profile = DeviceProfileFactory.Create(TestCapabilities.Full with { HlsContainers = [Codec.Container.Mp4] }, _defaults);

        var video = Assert.Single(profile.TranscodingProfiles!, p => p.Type == TranscodingProfile_Type.Video);
        Assert.Equal("mp4", video.Container);
        Assert.Equal(TranscodingProfile_Protocol.Hls, video.Protocol);
        Assert.Equal("hevc,h264", video.VideoCodec);
        Assert.Contains("flac", video.AudioCodec, StringComparison.Ordinal);
    }
}
