using Android.Content;
using Android.Media;
using Android.OS;
using Jellyfin.Mvvm.Playback.Profile;

namespace Jellyfin.Avalonia.Android.Profile;

/// <summary>
/// Detects codec capabilities like the Jellyfin Android TV client's <c>MediaCodecCapabilitiesTest</c>.
/// </summary>
internal static class AndroidMediaCapabilities
{
    private const string MimeAvc = "video/avc";
    private const string MimeHevc = "video/hevc";
    private const string MimeAv1 = "video/av01";
    private const string MimeVc1 = "video/wvc1";
    private const string MimeDolbyVision = "video/dolby-vision";

    // MediaCodecInfo.CodecProfileLevel constants
    private const int AvcProfileMain = 0x02;
    private const int AvcProfileHigh10 = 0x10;
    private const int AvcLevel4 = 0x800;
    private const int HevcProfileMain = 0x01;
    private const int HevcProfileMain10 = 0x02;
    private const int HevcProfileMain10Hdr10 = 0x1000;
    private const int HevcProfileMain10Hdr10Plus = 0x2000;
    private const int HevcMainTierLevel4 = 0x400;
    private const int Av1ProfileMain10 = 0x2;
    private const int Av1ProfileMain10Hdr10 = 0x1000;
    private const int Av1ProfileMain10Hdr10Plus = 0x2000;
    private const int Av1Level5 = 0x1000;
    private const int DolbyVisionProfileDvheDtb = 0x80;
    private const int DolbyVisionProfileDvav110 = 0x400;
    private const int DolbyVisionLevelHd24 = 0x4;

    // AVC levels as reported by ffprobe are multiplied by 10, e.g. level 4.1 is 41. Level 1b is set to 9.
    private static readonly (int CodecLevel, int Level)[] _avcLevels =
    [
        (0x02, 9), (0x01, 10), (0x04, 11), (0x08, 12), (0x10, 13), (0x20, 20), (0x40, 21), (0x80, 22),
        (0x100, 30), (0x200, 31), (0x400, 32), (0x800, 40), (0x1000, 41), (0x2000, 42), (0x4000, 50),
        (0x8000, 51), (0x10000, 52),
    ];

    // HEVC levels as reported by ffprobe are multiplied by 30, e.g. level 4.1 is 123.
    private static readonly (int CodecLevel, int Level)[] _hevcLevels =
    [
        (0x1, 30), (0x4, 60), (0x10, 63), (0x40, 90), (0x100, 93), (0x400, 120), (0x1000, 123), (0x4000, 150),
        (0x10000, 153), (0x40000, 156), (0x100000, 180), (0x400000, 183), (0x1000000, 186),
    ];

    // Device models with known HEVC Dolby Vision / HDR10+ playback issues.
    private static readonly string[] _modelsWithDoViHdr10PlusBug = ["AFTKRT", "AFTKA", "AFTKM", "AFTMM"];

    // Device models with unreported Dolby Vision profile 7 support.
    private static readonly string[] _modelsWithUnreportedDoviProfile7Support = ["SHIELD Android TV"];

    /// <summary>
    /// Detect the capabilities.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="softwareCodecsEnabled">Whether software decoders count as supported.</param>
    /// <param name="baseCapabilities">The libVLC capabilities to refine.</param>
    /// <returns>The capabilities.</returns>
    public static MediaCapabilities Detect(Context context, bool softwareCodecsEnabled, MediaCapabilities baseCapabilities)
    {
        var query = new MediaCodecQuery(softwareCodecsEnabled);
        var atLeastN = Build.VERSION.SdkInt >= BuildVersionCodes.N;
        var supportsHevcMain10 = query.HasDecoder(MimeHevc, HevcProfileMain10, HevcMainTierLevel4);

        return baseCapabilities with
        {
            Name = AppConstants.Name + " Android",
            SupportsAvc = query.HasCodecForMime(MimeAvc),
            SupportsAvcHigh10 = query.HasDecoder(MimeAvc, AvcProfileHigh10, AvcLevel4),
            AvcMainLevel = MapLevel(query.GetDecoderLevel(MimeAvc, AvcProfileMain), _avcLevels),
            AvcHigh10Level = MapLevel(query.GetDecoderLevel(MimeAvc, AvcProfileHigh10), _avcLevels),
            AvcMaxResolution = query.GetMaxResolution(MimeAvc),
            SupportsHevc = query.HasCodecForMime(MimeHevc),
            SupportsHevcMain10 = supportsHevcMain10,
            HevcMainLevel = MapLevel(query.GetDecoderLevel(MimeHevc, HevcProfileMain), _hevcLevels),
            HevcMain10Level = MapLevel(query.GetDecoderLevel(MimeHevc, HevcProfileMain10), _hevcLevels),
            HevcMaxResolution = query.GetMaxResolution(MimeHevc),
            SupportsHevcDolbyVision = atLeastN && query.HasCodecForMime(MimeDolbyVision),
            SupportsHevcDolbyVisionEl = atLeastN
                && query.HasDecoder(MimeDolbyVision, DolbyVisionProfileDvheDtb, DolbyVisionLevelHd24)
                && query.SupportsMultiInstance(MimeHevc),
            SupportsHevcHdr10 = atLeastN && query.HasDecoder(MimeHevc, HevcProfileMain10Hdr10, HevcMainTierLevel4),
            SupportsHevcHdr10Plus = Build.VERSION.SdkInt >= BuildVersionCodes.Q
                && query.HasDecoder(MimeHevc, HevcProfileMain10Hdr10Plus, HevcMainTierLevel4),
            SupportsAv1 = query.HasCodecForMime(MimeAv1),
            SupportsAv1Main10 = query.HasDecoder(MimeAv1, Av1ProfileMain10, Av1Level5),
            SupportsAv1DolbyVision = atLeastN && query.HasDecoder(MimeDolbyVision, DolbyVisionProfileDvav110, DolbyVisionLevelHd24),
            SupportsAv1Hdr10 = query.HasDecoder(MimeAv1, Av1ProfileMain10Hdr10, Av1Level5),
            SupportsAv1Hdr10Plus = query.HasDecoder(MimeAv1, Av1ProfileMain10Hdr10Plus, Av1Level5),
            Av1MaxResolution = query.GetMaxResolution(MimeAv1),
            SupportsVc1 = query.HasCodecForMime(MimeVc1),
            Vc1MaxResolution = query.GetMaxResolution(MimeVc1),
            HasHevcDoviHdr10PlusBug = _modelsWithDoViHdr10PlusBug.Contains(Build.Model),
            HasUnreportedDoviProfile7Support = _modelsWithUnreportedDoviProfile7Support.Contains(Build.Model),
            PassthroughAudioCodecs = GetPassthroughCodecs(context),
        };
    }

    private static int MapLevel(int codecLevel, (int CodecLevel, int Level)[] map)
    {
        for (var i = map.Length - 1; i >= 0; i--)
        {
            if (codecLevel >= map[i].CodecLevel)
            {
                return map[i].Level;
            }
        }

        return 0;
    }

    private static HashSet<string> GetPassthroughCodecs(Context context)
    {
        // AudioDeviceInfo and AudioFormat constants, compared as integers to cover all API levels.
        const int TypeHdmi = 9;
        const int TypeHdmiArc = 10;
        const int TypeHdmiEarc = 29;
        const int EncodingAc3 = 5;
        const int EncodingEac3 = 6;
        const int EncodingDts = 7;
        const int EncodingDtsHd = 8;
        const int EncodingTrueHd = 14;
        const int EncodingEac3Joc = 18;

        var codecs = new HashSet<string>(StringComparer.Ordinal);
        if (!OperatingSystem.IsAndroidVersionAtLeast(23)
            || context.GetSystemService(Context.AudioService) is not AudioManager audioManager)
        {
            return codecs;
        }

        foreach (var device in audioManager.GetDevices(GetDevicesTargets.Outputs) ?? [])
        {
            if ((int)device.Type is not (TypeHdmi or TypeHdmiArc or TypeHdmiEarc))
            {
                continue;
            }

            foreach (var encoding in device.GetEncodings() ?? [])
            {
                switch ((int)encoding)
                {
                    case EncodingAc3:
                        codecs.Add(Codec.Audio.Ac3);
                        break;
                    case EncodingEac3 or EncodingEac3Joc:
                        codecs.Add(Codec.Audio.Eac3);
                        break;
                    case EncodingDts or EncodingDtsHd:
                        codecs.Add(Codec.Audio.Dts);
                        break;
                    case EncodingTrueHd:
                        codecs.Add(Codec.Audio.TrueHd);
                        break;
                }
            }
        }

        return codecs;
    }
}
