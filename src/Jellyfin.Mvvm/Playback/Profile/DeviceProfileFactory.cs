using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Mvvm.Playback.Profile;

/// <summary>
/// Builds a <see cref="DeviceProfile"/> from the device's <see cref="MediaCapabilities"/> and the user's <see cref="PlaybackPreferences"/>.
/// </summary>
/// <remarks>
/// This follows the profile built by the Jellyfin Android TV client: the device is queried for what it can decode,
/// the user can override detected capabilities, and the server is told exactly when to transcode.
/// </remarks>
public static class DeviceProfileFactory
{
    private static readonly string[] _downmixSupportedAudioCodecs =
    [
        Codec.Audio.Aac,
        Codec.Audio.Mp2,
        Codec.Audio.Mp3,
    ];

    private static readonly string[] _hlsMpegTsAudioCodecs =
    [
        Codec.Audio.Aac,
        Codec.Audio.Ac3,
        Codec.Audio.Eac3,
        Codec.Audio.Mp3,
    ];

    private static readonly string[] _hlsFmp4AudioCodecs =
    [
        Codec.Audio.Aac,
        Codec.Audio.Ac3,
        Codec.Audio.Eac3,
        Codec.Audio.Mp3,
        Codec.Audio.Alac,
        Codec.Audio.Flac,
        Codec.Audio.Opus,
        Codec.Audio.Dts,
    ];

    /// <summary>
    /// Create the device profile.
    /// </summary>
    /// <param name="capabilities">The device capabilities.</param>
    /// <param name="preferences">The playback preferences.</param>
    /// <returns>The device profile.</returns>
    public static DeviceProfile Create(MediaCapabilities capabilities, PlaybackPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(preferences);

        var downMixAudio = preferences.AudioBehavior == AudioBehavior.DownmixToStereo;
        var allowedAudioCodecs = GetAllowedAudioCodecs(capabilities, preferences, downMixAudio);
        var maxBitrate = preferences.GetMaxBitrate();

        var avcMainLevel = preferences.AvcLevel ?? capabilities.AvcMainLevel;
        var avcHigh10Level = preferences.AvcLevel ?? capabilities.AvcHigh10Level;
        var hevcMainLevel = preferences.HevcLevel ?? capabilities.HevcMainLevel;
        var hevcMain10Level = preferences.HevcLevel ?? capabilities.HevcMain10Level;

        var profile = new DeviceProfile
        {
            Name = capabilities.Name,
            MaxStaticBitrate = maxBitrate,
            MaxStreamingBitrate = maxBitrate,
            TranscodingProfiles = [],
            DirectPlayProfiles = [],
            CodecProfiles = [],
            ContainerProfiles = [],
            SubtitleProfiles = [],
        };

        AddTranscodingProfiles(profile, capabilities, allowedAudioCodecs);

        profile.DirectPlayProfiles.Add(new DirectPlayProfile
        {
            Type = DirectPlayProfile_Type.Video,
            Container = Join(capabilities.DirectPlayVideoContainers),
            VideoCodec = Join(capabilities.DirectPlayVideoCodecs),
            AudioCodec = Join(allowedAudioCodecs),
        });

        profile.DirectPlayProfiles.Add(new DirectPlayProfile
        {
            Type = DirectPlayProfile_Type.Audio,
            Container = capabilities.DirectPlayAudioContainers.Count == 0 ? null : Join(capabilities.DirectPlayAudioContainers),
            AudioCodec = Join(allowedAudioCodecs),
        });

        profile.CodecProfiles.Add(VideoCodecProfile(
            Codec.Video.H264,
            [
                capabilities.SupportsAvc
                    ? Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.EqualsAny, JoinNotNull("high", "main", "baseline", "constrained baseline", capabilities.SupportsAvcHigh10 ? "high 10" : null))
                    : Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "none")
            ]));

        if (capabilities.SupportsAvc)
        {
            profile.CodecProfiles.Add(VideoCodecProfile(
                Codec.Video.H264,
                [Condition(ProfileCondition_Property.VideoLevel, ProfileCondition_Condition.LessThanEqual, avcMainLevel)],
                [Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.EqualsAny, "high|main|baseline|constrained baseline")]));
        }

        if (capabilities.SupportsAvcHigh10)
        {
            profile.CodecProfiles.Add(VideoCodecProfile(
                Codec.Video.H264,
                [Condition(ProfileCondition_Property.VideoLevel, ProfileCondition_Condition.LessThanEqual, avcHigh10Level)],
                [Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "high 10")]));
        }

        profile.CodecProfiles.Add(VideoCodecProfile(
            Codec.Video.H264,
            [Condition(ProfileCondition_Property.RefFrames, ProfileCondition_Condition.LessThanEqual, 12)],
            [Condition(ProfileCondition_Property.Width, ProfileCondition_Condition.GreaterThanEqual, 1200)]));

        profile.CodecProfiles.Add(VideoCodecProfile(
            Codec.Video.H264,
            [Condition(ProfileCondition_Property.RefFrames, ProfileCondition_Condition.LessThanEqual, 4)],
            [Condition(ProfileCondition_Property.Width, ProfileCondition_Condition.GreaterThanEqual, 1900)]));

        profile.CodecProfiles.Add(VideoCodecProfile(
            Codec.Video.Hevc,
            [
                capabilities.SupportsHevc
                    ? Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.EqualsAny, JoinNotNull("main", capabilities.SupportsHevcMain10 ? "main 10" : null))
                    : Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "none")
            ]));

        if (capabilities.SupportsHevc)
        {
            profile.CodecProfiles.Add(VideoCodecProfile(
                Codec.Video.Hevc,
                [Condition(ProfileCondition_Property.VideoLevel, ProfileCondition_Condition.LessThanEqual, hevcMainLevel)],
                [Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "main")]));
        }

        if (capabilities.SupportsHevcMain10)
        {
            profile.CodecProfiles.Add(VideoCodecProfile(
                Codec.Video.Hevc,
                [Condition(ProfileCondition_Property.VideoLevel, ProfileCondition_Condition.LessThanEqual, hevcMain10Level)],
                [Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "main 10")]));
        }

        profile.CodecProfiles.Add(VideoCodecProfile(
            Codec.Video.Av1,
            [
                !capabilities.SupportsAv1
                    ? Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "none")
                    : !capabilities.SupportsAv1Main10
                        ? Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.NotEquals, "main 10")
                        : Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.NotEquals, "none")
            ]));

        profile.CodecProfiles.Add(VideoCodecProfile(
            Codec.Video.Vc1,
            [
                capabilities.SupportsVc1
                    ? Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.NotEquals, "none")
                    : Condition(ProfileCondition_Property.VideoProfile, ProfileCondition_Condition.Equals, "none")
            ]));

        profile.CodecProfiles.Add(ResolutionProfile(Codec.Video.H264, capabilities.AvcMaxResolution));
        profile.CodecProfiles.Add(ResolutionProfile(Codec.Video.Hevc, capabilities.HevcMaxResolution));
        profile.CodecProfiles.Add(ResolutionProfile(Codec.Video.Av1, capabilities.Av1MaxResolution));
        profile.CodecProfiles.Add(ResolutionProfile(Codec.Video.Vc1, capabilities.Vc1MaxResolution));

        var (forceEnabledHdr, forceDisabledHdr) = GetHdrOverrides(preferences);
        AddRangeTypeProfile(profile, Codec.Video.Av1, ApplyOverrides(GetUnsupportedAv1RangeTypes(capabilities), forceEnabledHdr, forceDisabledHdr));
        AddRangeTypeProfile(profile, Codec.Video.Hevc, ApplyOverrides(GetUnsupportedHevcRangeTypes(capabilities), forceEnabledHdr, forceDisabledHdr));

        profile.CodecProfiles.Add(new CodecProfile
        {
            Type = CodecProfile_Type.VideoAudio,
            Conditions =
            [
                Condition(ProfileCondition_Property.AudioChannels, ProfileCondition_Condition.LessThanEqual, downMixAudio ? 2 : Math.Min(capabilities.MaxAudioChannels, 8))
            ],
        });

        AddSubtitleProfiles(profile, capabilities, preferences);

        return profile;
    }

    internal static IReadOnlyList<string> GetAllowedAudioCodecs(MediaCapabilities capabilities, PlaybackPreferences preferences, bool downMixAudio)
    {
        var candidates = new List<string>();
        foreach (var codec in capabilities.DecodableAudioCodecs.Concat(capabilities.PassthroughAudioCodecs))
        {
            if (!candidates.Contains(codec, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(codec);
            }
        }

        if (downMixAudio)
        {
            return candidates.Where(c => _downmixSupportedAudioCodecs.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        return candidates.Where(codec => GetBitstreamMode(codec, preferences) switch
            {
                BitstreamAudioMode.Enable => true,
                BitstreamAudioMode.Disable => false,
                _ => capabilities.DecodableAudioCodecs.Contains(codec, StringComparer.OrdinalIgnoreCase)
                     || (preferences.AudioPassthrough && capabilities.PassthroughAudioCodecs.Contains(codec, StringComparer.OrdinalIgnoreCase))
            })
            .ToList();
    }

    internal static HashSet<string> GetUnsupportedAv1RangeTypes(MediaCapabilities capabilities)
    {
        var set = new HashSet<string>(StringComparer.Ordinal) { VideoRangeTypes.DoviInvalid };

        if (!capabilities.SupportsAv1DolbyVision)
        {
            set.Add(VideoRangeTypes.Dovi);
            if (!capabilities.SupportsAv1Hdr10)
            {
                set.Add(VideoRangeTypes.DoviWithHdr10);
            }

            if (!capabilities.SupportsAv1Hdr10Plus)
            {
                set.Add(VideoRangeTypes.DoviWithHdr10Plus);
            }
        }

        if (!capabilities.SupportsAv1Hdr10Plus)
        {
            set.Add(VideoRangeTypes.Hdr10Plus);

            if (!capabilities.SupportsAv1Hdr10)
            {
                set.Add(VideoRangeTypes.Hdr10);
            }
        }

        return set;
    }

    internal static HashSet<string> GetUnsupportedHevcRangeTypes(MediaCapabilities capabilities)
    {
        var set = new HashSet<string>(StringComparer.Ordinal) { VideoRangeTypes.DoviInvalid };

        if (!capabilities.SupportsHevcDolbyVisionEl)
        {
            if (!capabilities.HasUnreportedDoviProfile7Support
                || !capabilities.SupportsHevcDolbyVision
                || !capabilities.SupportsHevcMain10
                || !capabilities.SupportsHevcHdr10)
            {
                set.Add(VideoRangeTypes.DoviWithEl);

                if (!capabilities.SupportsHevcHdr10Plus && !capabilities.HasHevcDoviHdr10PlusBug)
                {
                    set.Add(VideoRangeTypes.DoviWithElHdr10Plus);
                }
            }

            if (!capabilities.SupportsHevcDolbyVision)
            {
                set.Add(VideoRangeTypes.Dovi);
                if (!capabilities.SupportsHevcHdr10)
                {
                    set.Add(VideoRangeTypes.DoviWithHdr10);
                }

                if (!capabilities.SupportsHevcHdr10Plus && !capabilities.HasHevcDoviHdr10PlusBug)
                {
                    set.Add(VideoRangeTypes.DoviWithHdr10Plus);
                }
            }
        }

        if (!capabilities.SupportsHevcHdr10Plus)
        {
            set.Add(VideoRangeTypes.Hdr10Plus);
            if (!capabilities.SupportsHevcHdr10)
            {
                set.Add(VideoRangeTypes.Hdr10);
            }
        }

        if (capabilities.HasHevcDoviHdr10PlusBug)
        {
            set.Add(VideoRangeTypes.DoviWithHdr10Plus);
            set.Add(VideoRangeTypes.DoviWithElHdr10Plus);
        }

        return set;
    }

    private static void AddTranscodingProfiles(DeviceProfile profile, MediaCapabilities capabilities, IReadOnlyList<string> allowedAudioCodecs)
    {
        var hlsVideoCodecs = capabilities.SupportsHevc
            ? new[] { Codec.Video.Hevc, Codec.Video.H264 }
            : [Codec.Video.H264];

        foreach (var container in capabilities.HlsContainers)
        {
            var audioCodecs = string.Equals(container, Codec.Container.Mp4, StringComparison.OrdinalIgnoreCase)
                ? _hlsFmp4AudioCodecs
                : _hlsMpegTsAudioCodecs;

            var allowed = audioCodecs.Where(c => allowedAudioCodecs.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
            if (allowed.Count == 0)
            {
                // AAC is always decodable after transcoding.
                allowed.Add(Codec.Audio.Aac);
            }

            profile.TranscodingProfiles!.Add(new TranscodingProfile
            {
                Type = TranscodingProfile_Type.Video,
                Context = TranscodingProfile_Context.Streaming,
                Container = container,
                Protocol = TranscodingProfile_Protocol.Hls,
                VideoCodec = Join(hlsVideoCodecs),
                AudioCodec = Join(allowed),
                CopyTimestamps = false,
                EnableSubtitlesInManifest = true,
            });
        }

        profile.TranscodingProfiles!.Add(new TranscodingProfile
        {
            Type = TranscodingProfile_Type.Audio,
            Context = TranscodingProfile_Context.Streaming,
            Container = Codec.Container.Ts,
            Protocol = TranscodingProfile_Protocol.Hls,
            AudioCodec = Codec.Audio.Aac,
        });
    }

    private static void AddSubtitleProfiles(DeviceProfile profile, MediaCapabilities capabilities, PlaybackPreferences preferences)
    {
        var embedded = capabilities.SupportsEmbeddedSubtitles;
        var external = capabilities.SupportsExternalSubtitles;

        bool IsExternalFormat(string format) => external && capabilities.ExternalTextSubtitleFormats.Contains(format, StringComparer.OrdinalIgnoreCase);

        // The server only supports WebVTT subtitles in HLS, other text subtitles would be converted to WebVTT
        // which we do not want so only allow delivery over HLS for WebVTT subtitles.
        AddSubtitleProfile(profile, Codec.Subtitle.Vtt, embedded: embedded, external: IsExternalFormat(Codec.Subtitle.Vtt), hls: true);
        AddSubtitleProfile(profile, Codec.Subtitle.WebVtt, embedded: embedded, external: IsExternalFormat(Codec.Subtitle.WebVtt), hls: true);

        AddSubtitleProfile(profile, Codec.Subtitle.Srt, embedded: embedded, external: IsExternalFormat(Codec.Subtitle.Srt));
        AddSubtitleProfile(profile, Codec.Subtitle.SubRip, embedded: embedded, external: IsExternalFormat(Codec.Subtitle.SubRip));
        AddSubtitleProfile(profile, Codec.Subtitle.Ttml, embedded: embedded, external: IsExternalFormat(Codec.Subtitle.Ttml));

        // Not all subtitles can be loaded standalone by the player
        var bitmapEmbedded = embedded && capabilities.SupportsBitmapSubtitles;
        AddSubtitleProfile(profile, Codec.Subtitle.DvbSub, embedded: bitmapEmbedded, encode: true);
        AddSubtitleProfile(profile, Codec.Subtitle.DvdSub, embedded: bitmapEmbedded, encode: true);
        AddSubtitleProfile(profile, Codec.Subtitle.Idx, embedded: bitmapEmbedded, encode: true);

        var pgsDirectPlay = capabilities.SupportsPgsSubtitles && preferences.PgsDirectPlay;
        var pgsExternal = pgsDirectPlay && external && capabilities.SupportsExternalPgsSubtitles;
        AddSubtitleProfile(profile, Codec.Subtitle.Pgs, embedded: pgsDirectPlay && embedded, external: pgsExternal, encode: true);
        AddSubtitleProfile(profile, Codec.Subtitle.PgsSub, embedded: pgsDirectPlay && embedded, external: pgsExternal, encode: true);

        // ASS/SSA is supported when the player has a libass renderer
        var assDirectPlay = capabilities.SupportsAssSubtitles && preferences.AssDirectPlay;
        AddSubtitleProfile(profile, Codec.Subtitle.Ass, encode: true, embedded: assDirectPlay && embedded, external: assDirectPlay && external);
        AddSubtitleProfile(profile, Codec.Subtitle.Ssa, encode: true, embedded: assDirectPlay && embedded, external: assDirectPlay && external);
    }

    private static void AddSubtitleProfile(
        DeviceProfile profile,
        string format,
        bool embedded = false,
        bool external = false,
        bool hls = false,
        bool encode = false)
    {
        if (embedded)
        {
            profile.SubtitleProfiles!.Add(new SubtitleProfile { Format = format, Method = SubtitleProfile_Method.Embed });
        }

        if (external)
        {
            profile.SubtitleProfiles!.Add(new SubtitleProfile { Format = format, Method = SubtitleProfile_Method.External });
        }

        if (hls)
        {
            profile.SubtitleProfiles!.Add(new SubtitleProfile { Format = format, Method = SubtitleProfile_Method.Hls });
        }

        if (encode)
        {
            profile.SubtitleProfiles!.Add(new SubtitleProfile { Format = format, Method = SubtitleProfile_Method.Encode });
        }
    }

    private static (HashSet<string> Enabled, HashSet<string> Disabled) GetHdrOverrides(PlaybackPreferences preferences)
    {
        var enabled = new HashSet<string>(StringComparer.Ordinal);
        var disabled = new HashSet<string>(StringComparer.Ordinal);

        void Apply(HdrOverrideMode mode, params string[] rangeTypes)
        {
            switch (mode)
            {
                case HdrOverrideMode.Enable:
                    enabled.UnionWith(rangeTypes);
                    break;
                case HdrOverrideMode.Disable:
                    disabled.UnionWith(rangeTypes);
                    break;
            }
        }

        Apply(preferences.Hdr10Override, VideoRangeTypes.Hdr10);
        Apply(preferences.Hdr10PlusOverride, VideoRangeTypes.Hdr10Plus);
        Apply(preferences.HlgOverride, VideoRangeTypes.Hlg);
        Apply(preferences.DoviProfile5Override, VideoRangeTypes.Dovi);
        Apply(preferences.DoviProfile7Override, VideoRangeTypes.DoviWithEl, VideoRangeTypes.DoviWithElHdr10Plus);
        Apply(preferences.DoviProfile8Override, VideoRangeTypes.DoviWithHdr10, VideoRangeTypes.DoviWithHdr10Plus, VideoRangeTypes.DoviWithHlg);

        return (enabled, disabled);
    }

    private static HashSet<string> ApplyOverrides(HashSet<string> unsupported, HashSet<string> forceEnabled, HashSet<string> forceDisabled)
    {
        unsupported.ExceptWith(forceEnabled);
        unsupported.UnionWith(forceDisabled);
        return unsupported;
    }

    // Applies only to media in an unsupported range type, where the server's ConditionProcessor always fails NotEquals.
    // StreamBuilder then turns the NotEquals values into the range types allowed in the transcode.
    private static void AddRangeTypeProfile(DeviceProfile profile, string codec, HashSet<string> unsupportedRangeTypes)
    {
        if (unsupportedRangeTypes.Count == 0)
        {
            return;
        }

        var ordered = unsupportedRangeTypes.Order(StringComparer.Ordinal).ToList();
        profile.CodecProfiles!.Add(VideoCodecProfile(
            codec,
            [Condition(ProfileCondition_Property.VideoRangeType, ProfileCondition_Condition.NotEquals, JoinNotNull([.. ordered]))],
            [Condition(ProfileCondition_Property.VideoRangeType, ProfileCondition_Condition.EqualsAny, JoinNotNull([.. ordered]))]));
    }

    private static CodecProfile ResolutionProfile(string codec, VideoResolution resolution)
        => VideoCodecProfile(
            codec,
            [
                Condition(ProfileCondition_Property.Width, ProfileCondition_Condition.LessThanEqual, resolution.Width),
                Condition(ProfileCondition_Property.Height, ProfileCondition_Condition.LessThanEqual, resolution.Height)
            ]);

    private static CodecProfile VideoCodecProfile(string codec, List<ProfileCondition> conditions, List<ProfileCondition>? applyConditions = null)
        => new()
        {
            Type = CodecProfile_Type.Video,
            Codec = codec,
            Conditions = conditions,
            ApplyConditions = applyConditions ?? [],
        };

    private static ProfileCondition Condition(ProfileCondition_Property property, ProfileCondition_Condition condition, string value)
        => new()
        {
            Property = property,
            Condition = condition,
            Value = value,
            IsRequired = false,
        };

    private static ProfileCondition Condition(ProfileCondition_Property property, ProfileCondition_Condition condition, int value)
        => Condition(property, condition, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static BitstreamAudioMode GetBitstreamMode(string codec, PlaybackPreferences preferences)
        => codec.ToLowerInvariant() switch
        {
            Codec.Audio.Ac3 => preferences.BitstreamAc3,
            Codec.Audio.Eac3 => preferences.BitstreamEac3,
            Codec.Audio.Dts or Codec.Audio.Dca => preferences.BitstreamDts,
            Codec.Audio.TrueHd or Codec.Audio.Mlp => preferences.BitstreamTrueHd,
            _ => BitstreamAudioMode.Auto
        };

    private static string Join(IEnumerable<string> values) => string.Join(',', values);

    private static string JoinNotNull(params string?[] values) => string.Join('|', values.Where(v => v is not null));
}
