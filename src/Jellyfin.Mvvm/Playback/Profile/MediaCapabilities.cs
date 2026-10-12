namespace Jellyfin.Mvvm.Playback.Profile;

/// <summary>
/// Describes what the current device and player backend can decode and render.
/// </summary>
/// <remarks>
/// Each platform reports its own capabilities, modelled after the media codec queries in the
/// Jellyfin Android TV client. The <see cref="DeviceProfileFactory"/> turns these capabilities
/// and the user's playback preferences into a device profile for the server.
/// </remarks>
public sealed record MediaCapabilities
{
    /// <summary>
    /// Gets the device profile name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a value indicating whether H.264 is supported.
    /// </summary>
    public bool SupportsAvc { get; init; }

    /// <summary>
    /// Gets a value indicating whether H.264 High 10 is supported.
    /// </summary>
    public bool SupportsAvcHigh10 { get; init; }

    /// <summary>
    /// Gets the maximum H.264 level for the main/high profiles, multiplied by 10 (e.g. 41 for level 4.1).
    /// </summary>
    public int AvcMainLevel { get; init; }

    /// <summary>
    /// Gets the maximum H.264 level for the High 10 profile, multiplied by 10.
    /// </summary>
    public int AvcHigh10Level { get; init; }

    /// <summary>
    /// Gets the maximum resolution of the H.264 decoder.
    /// </summary>
    public VideoResolution AvcMaxResolution { get; init; }

    /// <summary>
    /// Gets a value indicating whether HEVC is supported.
    /// </summary>
    public bool SupportsHevc { get; init; }

    /// <summary>
    /// Gets a value indicating whether HEVC Main 10 is supported.
    /// </summary>
    public bool SupportsHevcMain10 { get; init; }

    /// <summary>
    /// Gets the maximum HEVC level for the main profile, multiplied by 30 (e.g. 123 for level 4.1).
    /// </summary>
    public int HevcMainLevel { get; init; }

    /// <summary>
    /// Gets the maximum HEVC level for the main 10 profile, multiplied by 30.
    /// </summary>
    public int HevcMain10Level { get; init; }

    /// <summary>
    /// Gets the maximum resolution of the HEVC decoder.
    /// </summary>
    public VideoResolution HevcMaxResolution { get; init; }

    /// <summary>
    /// Gets a value indicating whether HEVC Dolby Vision is supported.
    /// </summary>
    public bool SupportsHevcDolbyVision { get; init; }

    /// <summary>
    /// Gets a value indicating whether HEVC Dolby Vision with an enhancement layer (profile 7) is supported.
    /// </summary>
    public bool SupportsHevcDolbyVisionEl { get; init; }

    /// <summary>
    /// Gets a value indicating whether HEVC HDR10 is supported.
    /// </summary>
    public bool SupportsHevcHdr10 { get; init; }

    /// <summary>
    /// Gets a value indicating whether HEVC HDR10+ is supported.
    /// </summary>
    public bool SupportsHevcHdr10Plus { get; init; }

    /// <summary>
    /// Gets a value indicating whether AV1 is supported.
    /// </summary>
    public bool SupportsAv1 { get; init; }

    /// <summary>
    /// Gets a value indicating whether AV1 Main 10 is supported.
    /// </summary>
    public bool SupportsAv1Main10 { get; init; }

    /// <summary>
    /// Gets the maximum resolution of the AV1 decoder.
    /// </summary>
    public VideoResolution Av1MaxResolution { get; init; }

    /// <summary>
    /// Gets a value indicating whether AV1 Dolby Vision is supported.
    /// </summary>
    public bool SupportsAv1DolbyVision { get; init; }

    /// <summary>
    /// Gets a value indicating whether AV1 HDR10 is supported.
    /// </summary>
    public bool SupportsAv1Hdr10 { get; init; }

    /// <summary>
    /// Gets a value indicating whether AV1 HDR10+ is supported.
    /// </summary>
    public bool SupportsAv1Hdr10Plus { get; init; }

    /// <summary>
    /// Gets a value indicating whether VC-1 is supported.
    /// </summary>
    public bool SupportsVc1 { get; init; }

    /// <summary>
    /// Gets the maximum resolution of the VC-1 decoder.
    /// </summary>
    public VideoResolution Vc1MaxResolution { get; init; }

    /// <summary>
    /// Gets a value indicating whether the device reports Dolby Vision profile 7 support incorrectly.
    /// </summary>
    public bool HasUnreportedDoviProfile7Support { get; init; }

    /// <summary>
    /// Gets a value indicating whether the device has known issues with HEVC Dolby Vision combined with HDR10+.
    /// </summary>
    public bool HasHevcDoviHdr10PlusBug { get; init; }

    /// <summary>
    /// Gets the video codecs that can be direct played.
    /// </summary>
    public required IReadOnlyCollection<string> DirectPlayVideoCodecs { get; init; }

    /// <summary>
    /// Gets the video containers that can be direct played.
    /// </summary>
    public required IReadOnlyCollection<string> DirectPlayVideoContainers { get; init; }

    /// <summary>
    /// Gets the audio-only containers that can be direct played, or an empty collection to allow any container.
    /// </summary>
    public IReadOnlyCollection<string> DirectPlayAudioContainers { get; init; } = [];

    /// <summary>
    /// Gets the audio codecs the player can decode itself.
    /// </summary>
    public required IReadOnlyCollection<string> DecodableAudioCodecs { get; init; }

    /// <summary>
    /// Gets the audio codecs the audio output can bitstream (passthrough) to a receiver.
    /// </summary>
    public IReadOnlyCollection<string> PassthroughAudioCodecs { get; init; } = [];

    /// <summary>
    /// Gets the maximum number of audio channels the device can output.
    /// </summary>
    public int MaxAudioChannels { get; init; } = 8;

    /// <summary>
    /// Gets the HLS segment containers supported for transcoded streams, in order of preference.
    /// </summary>
    public IReadOnlyList<string> HlsContainers { get; init; } = [Codec.Container.Ts, Codec.Container.Mp4];

    /// <summary>
    /// Gets a value indicating whether embedded subtitle streams can be rendered.
    /// </summary>
    public bool SupportsEmbeddedSubtitles { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether external text subtitles can be loaded side by side.
    /// </summary>
    public bool SupportsExternalSubtitles { get; init; } = true;

    /// <summary>
    /// Gets the text subtitle formats that can be loaded as external files.
    /// The server converts other text formats to one of these.
    /// </summary>
    public IReadOnlyCollection<string> ExternalTextSubtitleFormats { get; init; } =
    [
        Codec.Subtitle.Vtt,
        Codec.Subtitle.WebVtt,
        Codec.Subtitle.Srt,
        Codec.Subtitle.SubRip,
        Codec.Subtitle.Ttml,
    ];

    /// <summary>
    /// Gets a value indicating whether ASS/SSA subtitles can be rendered natively.
    /// </summary>
    public bool SupportsAssSubtitles { get; init; }

    /// <summary>
    /// Gets a value indicating whether PGS subtitles can be rendered natively.
    /// </summary>
    public bool SupportsPgsSubtitles { get; init; }

    /// <summary>
    /// Gets a value indicating whether PGS subtitles can be loaded as external files.
    /// </summary>
    public bool SupportsExternalPgsSubtitles { get; init; }

    /// <summary>
    /// Gets a value indicating whether DVD/DVB bitmap subtitles can be rendered natively when embedded.
    /// </summary>
    public bool SupportsBitmapSubtitles { get; init; } = true;
}
