using Android.Media;
using Jellyfin.Mvvm.Playback.Profile;

namespace Jellyfin.Avalonia.Android.Profile;

/// <summary>
/// Queries device decoder capabilities from <see cref="MediaCodecList"/>, ported from the Jellyfin Android TV client.
/// </summary>
internal sealed class MediaCodecQuery
{
    private readonly bool _softwareCodecsEnabled;
    private readonly Lazy<MediaCodecInfo[]> _decoders;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCodecQuery"/> class.
    /// </summary>
    /// <param name="softwareCodecsEnabled">Whether software decoders count as supported.</param>
    public MediaCodecQuery(bool softwareCodecsEnabled)
    {
        _softwareCodecsEnabled = softwareCodecsEnabled;
        _decoders = new Lazy<MediaCodecInfo[]>(LoadDecoders);
    }

    private IEnumerable<MediaCodecInfo> Decoders => _decoders.Value;

    /// <summary>
    /// Gets whether any decoder supports the mime type.
    /// </summary>
    /// <param name="mime">The mime type.</param>
    /// <returns>Whether a decoder exists.</returns>
    public bool HasCodecForMime(string mime)
        => Decoders.Any(info => (info.GetSupportedTypes() ?? []).Any(t => string.Equals(t, mime, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Gets whether a decoder supports the profile at or above the level.
    /// </summary>
    /// <param name="mime">The mime type.</param>
    /// <param name="profile">The profile constant.</param>
    /// <param name="level">The level constant.</param>
    /// <returns>Whether a decoder exists.</returns>
    public bool HasDecoder(string mime, int profile, int level)
        => Decoders.Any(info => GetProfileLevels(info, mime).Any(pl => pl.Profile == profile && pl.Level >= level));

    /// <summary>
    /// Gets the highest level supported for a profile.
    /// </summary>
    /// <param name="mime">The mime type.</param>
    /// <param name="profile">The profile constant.</param>
    /// <returns>The level constant, or 0.</returns>
    public int GetDecoderLevel(string mime, int profile)
        => Decoders.SelectMany(info => GetProfileLevels(info, mime))
            .Where(pl => pl.Profile == profile)
            .Select(pl => pl.Level)
            .DefaultIfEmpty(0)
            .Max();

    /// <summary>
    /// Gets the maximum supported resolution for the mime type.
    /// </summary>
    /// <param name="mime">The mime type.</param>
    /// <returns>The resolution.</returns>
    public VideoResolution GetMaxResolution(string mime)
    {
        var maxWidth = 0;
        var maxHeight = 0;
        foreach (var info in Decoders)
        {
            var video = GetCapabilities(info, mime)?.VideoCapabilities;
            if (video is null)
            {
                continue;
            }

            maxWidth = Math.Max(maxWidth, ToInt(video.SupportedWidths?.Upper));
            maxHeight = Math.Max(maxHeight, ToInt(video.SupportedHeights?.Upper));
        }

        return new VideoResolution(maxWidth, maxHeight);
    }

    /// <summary>
    /// Gets whether a decoder supports multiple concurrent instances.
    /// </summary>
    /// <param name="mime">The mime type.</param>
    /// <returns>Whether multiple instances are supported.</returns>
    public bool SupportsMultiInstance(string mime)
        => Decoders.Any(info => GetCapabilities(info, mime)?.MaxSupportedInstances > 1);

    private static int ToInt(Java.Lang.Object? value) => value is Java.Lang.Integer integer ? integer.IntValue() : 0;

    private static MediaCodecInfo.CodecCapabilities? GetCapabilities(MediaCodecInfo info, string mime)
    {
        if (!(info.GetSupportedTypes() ?? []).Any(t => string.Equals(t, mime, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        try
        {
            return info.GetCapabilitiesForType(mime);
        }
        catch (Java.Lang.IllegalArgumentException)
        {
            return null;
        }
    }

    private static IEnumerable<(int Profile, int Level)> GetProfileLevels(MediaCodecInfo info, string mime)
    {
        var capabilities = GetCapabilities(info, mime);
        if (capabilities?.ProfileLevels is null)
        {
            yield break;
        }

        foreach (var profileLevel in capabilities.ProfileLevels)
        {
            yield return ((int)profileLevel.Profile, (int)profileLevel.Level);
        }
    }

    private MediaCodecInfo[] LoadDecoders()
    {
        using var list = new MediaCodecList(MediaCodecListKind.RegularCodecs);
        return (list.GetCodecInfos() ?? [])
            .Where(info => !info.IsEncoder)
            .Where(info => _softwareCodecsEnabled || !IsSoftwareOnly(info))
            .ToArray();
    }

    private static bool IsSoftwareOnly(MediaCodecInfo info)
        => OperatingSystem.IsAndroidVersionAtLeast(29) && info.IsSoftwareOnly;
}
