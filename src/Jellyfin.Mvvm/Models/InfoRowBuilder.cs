using System.Globalization;

namespace Jellyfin.Mvvm.Models;

/// <summary>
/// Builds the metadata info row for an item, following the Jellyfin Android TV client.
/// </summary>
public static class InfoRowBuilder
{
    /// <summary>
    /// Build the info chips for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="now">The current time, used for the end time.</param>
    /// <param name="includeMediaDetails">Whether to include media stream details.</param>
    /// <returns>The chips.</returns>
    public static IReadOnlyList<InfoChip> Build(BaseItemDto item, DateTime now, bool includeMediaDetails = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        var chips = new List<InfoChip>();

        if (item.CommunityRating is { } communityRating and > 0)
        {
            chips.Add(new InfoChip(communityRating.ToString("0.0", CultureInfo.CurrentCulture), InfoChipStyle.Transparent, InfoChipIcon.Star));
        }

        if (item.CriticRating is { } criticRating and > 0)
        {
            chips.Add(new InfoChip(
                string.Create(CultureInfo.CurrentCulture, $"{criticRating:0}%"),
                InfoChipStyle.Transparent,
                criticRating >= 60 ? InfoChipIcon.CriticFresh : InfoChipIcon.CriticRotten));
        }

        var date = GetDate(item);
        if (date is not null)
        {
            chips.Add(new InfoChip(date));
        }

        if (item.Type == BaseItemDto_Type.Episode)
        {
            var number = Formatting.FormatEpisodeNumber(item);
            if (!string.IsNullOrEmpty(number))
            {
                chips.Add(new InfoChip(number));
            }
        }

        if (item.Type is not BaseItemDto_Type.Series and not BaseItemDto_Type.Season && Formatting.FormatRuntime(item.RunTimeTicks) is { } runtime)
        {
            chips.Add(new InfoChip(runtime, InfoChipStyle.Transparent, InfoChipIcon.Time));
        }

        if (item.Type == BaseItemDto_Type.Series && item.Status is not null)
        {
            if (string.Equals(item.Status, "Continuing", StringComparison.OrdinalIgnoreCase))
            {
                chips.Add(new InfoChip(Strings.Strings.Info_Continuing, InfoChipStyle.Green));
            }
            else if (string.Equals(item.Status, "Ended", StringComparison.OrdinalIgnoreCase))
            {
                chips.Add(new InfoChip(Strings.Strings.Info_Ended, InfoChipStyle.Red));
            }
            else if (string.Equals(item.Status, "Unreleased", StringComparison.OrdinalIgnoreCase))
            {
                chips.Add(new InfoChip(Strings.Strings.Info_Unreleased, InfoChipStyle.Default));
            }
        }

        if (!string.IsNullOrEmpty(item.OfficialRating))
        {
            chips.Add(new InfoChip(item.OfficialRating, InfoChipStyle.Default));
        }

        if (includeMediaDetails)
        {
            var mediaSource = item.MediaSources?.FirstOrDefault();
            var streams = mediaSource?.MediaStreams ?? item.MediaStreams;
            if (streams is not null)
            {
                AddMediaDetails(chips, streams, mediaSource?.DefaultAudioStreamIndex);
            }
        }

        if (item.MediaType == BaseItemDto_MediaType.Video && item.RunTimeTicks is { } ticks and > 0)
        {
            var remaining = ticks - (item.UserData?.PlaybackPositionTicks ?? 0);
            if (Formatting.FormatEndsAt(remaining, now) is { } endsAt)
            {
                chips.Add(new InfoChip(endsAt));
            }
        }

        return chips;
    }

    /// <summary>
    /// Get the video range display names for a range type.
    /// </summary>
    /// <param name="rangeType">The video range type.</param>
    /// <returns>The display names.</returns>
    public static IReadOnlyList<string> GetVideoRangeNames(MediaStream_VideoRangeType? rangeType) => rangeType switch
    {
        MediaStream_VideoRangeType.HDR10 => [Strings.Strings.Info_Hdr10],
        MediaStream_VideoRangeType.HDR10Plus => [Strings.Strings.Info_Hdr10Plus],
        MediaStream_VideoRangeType.HLG => [Strings.Strings.Info_Hlg],
        MediaStream_VideoRangeType.DOVI => [Strings.Strings.Info_DolbyVision],
        MediaStream_VideoRangeType.DOVIWithHDR10 => [Strings.Strings.Info_DolbyVision, Strings.Strings.Info_Hdr10],
        MediaStream_VideoRangeType.DOVIWithHLG => [Strings.Strings.Info_DolbyVision, Strings.Strings.Info_Hlg],
        MediaStream_VideoRangeType.DOVIWithSDR => [Strings.Strings.Info_DolbyVision, Strings.Strings.Info_Sdr],
        MediaStream_VideoRangeType.DOVIWithEL => [Strings.Strings.Info_DolbyVisionEl],
        MediaStream_VideoRangeType.DOVIWithHDR10Plus => [Strings.Strings.Info_DolbyVision, Strings.Strings.Info_Hdr10Plus],
        MediaStream_VideoRangeType.DOVIWithELHDR10Plus => [Strings.Strings.Info_DolbyVisionEl, Strings.Strings.Info_Hdr10Plus],
        MediaStream_VideoRangeType.DOVIInvalid => [Strings.Strings.Info_DolbyVision],
        _ => []
    };

    /// <summary>
    /// Get the display name of an audio stream codec.
    /// </summary>
    /// <param name="stream">The audio stream.</param>
    /// <returns>The display name.</returns>
    public static string? GetAudioCodecName(MediaStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var codec = stream.Codec?.ToUpperInvariant();
        var profile = stream.Profile ?? string.Empty;

        if (profile.Contains("Dolby Atmos", StringComparison.OrdinalIgnoreCase))
        {
            return codec switch
            {
                "EAC3" => "DD+ Atmos",
                "TRUEHD" => "TrueHD Atmos",
                _ => "Dolby Atmos"
            };
        }

        if (profile.Contains("DTS:X", StringComparison.OrdinalIgnoreCase))
        {
            return "DTS:X";
        }

        if (profile.Contains("DTS-HD MA", StringComparison.OrdinalIgnoreCase))
        {
            return "DTS-HD MA";
        }

        return codec switch
        {
            "AC3" => "DD",
            "EAC3" => "DD+",
            "TRUEHD" => "TrueHD",
            "DCA" or "DTS" => "DTS",
            null => null,
            _ => codec
        };
    }

    private static void AddMediaDetails(List<InfoChip> chips, IReadOnlyList<MediaStream> streams, int? defaultAudioIndex)
    {
        var video = streams.FirstOrDefault(s => s.Type == MediaStream_Type.Video);
        var audio = (defaultAudioIndex is not null
                ? streams.FirstOrDefault(s => s.Index == defaultAudioIndex && s.Type == MediaStream_Type.Audio)
                : null)
            ?? streams.FirstOrDefault(s => s.Type == MediaStream_Type.Audio);

        if (streams.Any(s => s.Type == MediaStream_Type.Subtitle && s.IsHearingImpaired == true))
        {
            chips.Add(new InfoChip(Strings.Strings.Info_SubtitlesHearingImpaired, InfoChipStyle.Default));
        }

        if (streams.Any(s => s.Type == MediaStream_Type.Subtitle && s.IsHearingImpaired != true))
        {
            chips.Add(new InfoChip(Strings.Strings.Info_Subtitles, InfoChipStyle.Default));
        }

        if (video?.Width is { } width && video.Height is { } height)
        {
            chips.Add(new InfoChip(Formatting.GetResolutionName(width, height, video.IsInterlaced == true), InfoChipStyle.Default));
        }

        if (video is not null)
        {
            foreach (var name in GetVideoRangeNames(video.VideoRangeType))
            {
                chips.Add(new InfoChip(name, InfoChipStyle.Default));
            }
        }

        if (audio is not null && GetAudioCodecName(audio) is { } audioName)
        {
            chips.Add(new InfoChip(audioName, InfoChipStyle.Default));
        }

        if (audio?.Channels is { } channels and > 0)
        {
            var layout = channels switch
            {
                1 => "1.0",
                2 => "2.0",
                3 => "2.1",
                6 => "5.1",
                7 => "6.1",
                8 => "7.1",
                _ => channels.ToString(CultureInfo.CurrentCulture) + "ch"
            };
            chips.Add(new InfoChip(layout, InfoChipStyle.Default));
        }
    }

    private static string? GetDate(BaseItemDto item)
    {
        switch (item.Type)
        {
            case BaseItemDto_Type.Person:
                if (item.PremiereDate is { } birthDate)
                {
                    var end = item.EndDate ?? DateTimeOffset.Now;
                    var age = end.Year - birthDate.Year - (end.DayOfYear < birthDate.DayOfYear ? 1 : 0);
                    return string.Format(CultureInfo.CurrentCulture, Strings.Strings.Item_Born, birthDate.ToString("d", CultureInfo.CurrentCulture)) + $" ({age.ToString(CultureInfo.CurrentCulture)})";
                }

                return null;
            case BaseItemDto_Type.Series:
                return Formatting.FormatYearRange(item);
            default:
                if (item.PremiereDate is { } premiereDate && item.Type == BaseItemDto_Type.Episode)
                {
                    return premiereDate.ToString("d", CultureInfo.CurrentCulture);
                }

                return item.ProductionYear?.ToString(CultureInfo.CurrentCulture)
                    ?? item.PremiereDate?.Year.ToString(CultureInfo.CurrentCulture);
        }
    }
}
