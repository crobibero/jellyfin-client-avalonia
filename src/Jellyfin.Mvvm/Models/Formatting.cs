using System.Globalization;

namespace Jellyfin.Mvvm.Models;

/// <summary>
/// Formatting helpers for item metadata.
/// </summary>
public static class Formatting
{
    /// <summary>
    /// Format a duration as h:mm:ss or m:ss.
    /// </summary>
    /// <param name="duration">The duration.</param>
    /// <returns>The formatted duration.</returns>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : duration.ToString(@"m\:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format a runtime as "1h 32m".
    /// </summary>
    /// <param name="runTimeTicks">The runtime in ticks.</param>
    /// <returns>The formatted runtime, or <c>null</c>.</returns>
    public static string? FormatRuntime(long? runTimeTicks)
    {
        if (runTimeTicks is null or <= 0)
        {
            return null;
        }

        var runtime = TimeSpan.FromTicks(runTimeTicks.Value);
        var hours = (int)runtime.TotalHours;
        var minutes = runtime.Minutes;

        if (hours == 0 && minutes == 0)
        {
            return string.Create(CultureInfo.CurrentCulture, $"{runtime.Seconds}s");
        }

        return hours > 0
            ? string.Create(CultureInfo.CurrentCulture, $"{hours}h {minutes}m")
            : string.Create(CultureInfo.CurrentCulture, $"{minutes}m");
    }

    /// <summary>
    /// Format the episode number as "S1:E2".
    /// </summary>
    /// <param name="item">The episode.</param>
    /// <returns>The episode number, or an empty string.</returns>
    public static string FormatEpisodeNumber(BaseItemDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.IndexNumber is null)
        {
            return string.Empty;
        }

        var episode = item.IndexNumberEnd is not null && item.IndexNumberEnd != item.IndexNumber
            ? string.Create(CultureInfo.CurrentCulture, $"E{item.IndexNumber}-{item.IndexNumberEnd}")
            : string.Create(CultureInfo.CurrentCulture, $"E{item.IndexNumber}");

        return item.ParentIndexNumber is null
            ? episode
            : string.Create(CultureInfo.CurrentCulture, $"S{item.ParentIndexNumber}:{episode}");
    }

    /// <summary>
    /// Format an episode title as "S1:E2 - Name".
    /// </summary>
    /// <param name="item">The episode.</param>
    /// <returns>The episode title.</returns>
    public static string? FormatEpisodeTitle(BaseItemDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var number = FormatEpisodeNumber(item);
        if (string.IsNullOrEmpty(number))
        {
            return item.Name;
        }

        return string.IsNullOrEmpty(item.Name) ? number : $"{number} - {item.Name}";
    }

    /// <summary>
    /// Format the year range of a series.
    /// </summary>
    /// <param name="item">The series.</param>
    /// <returns>The year range, or <c>null</c>.</returns>
    public static string? FormatYearRange(BaseItemDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.ProductionYear is null)
        {
            return null;
        }

        var start = item.ProductionYear.Value.ToString(CultureInfo.CurrentCulture);
        if (string.Equals(item.Status, "Continuing", StringComparison.OrdinalIgnoreCase))
        {
            return $"{start} – {Strings.Strings.Info_Present}";
        }

        if (item.EndDate is not null && item.EndDate.Value.Year != item.ProductionYear)
        {
            return $"{start} – {item.EndDate.Value.Year.ToString(CultureInfo.CurrentCulture)}";
        }

        return start;
    }

    /// <summary>
    /// Format the time an item would finish if started now.
    /// </summary>
    /// <param name="runTimeTicks">The remaining runtime in ticks.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The end time, or <c>null</c>.</returns>
    public static string? FormatEndsAt(long? runTimeTicks, DateTime now)
    {
        if (runTimeTicks is null or <= 0)
        {
            return null;
        }

        var end = now + TimeSpan.FromTicks(runTimeTicks.Value);
        return string.Format(CultureInfo.CurrentCulture, Strings.Strings.Item_EndsAt, end.ToString("t", CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Get a display name for a video resolution, e.g. "4K" or "1080p".
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="interlaced">Whether the video is interlaced.</param>
    /// <returns>The resolution name.</returns>
    public static string GetResolutionName(int width, int height, bool interlaced = false)
    {
        var suffix = interlaced ? "i" : "p";
        return (width, height) switch
        {
            _ when width >= 7600 || height >= 4300 => "8K",
            _ when width >= 3800 || height >= 2000 => "4K",
            _ when width >= 2500 || height >= 1400 => "1440" + suffix,
            _ when width >= 1800 || height >= 1000 => "1080" + suffix,
            _ when width >= 1200 || height >= 700 => "720" + suffix,
            _ when width >= 700 || height >= 500 => "480" + suffix,
            _ => "SD"
        };
    }

    /// <summary>
    /// Format a bitrate in bits per second.
    /// </summary>
    /// <param name="bitrate">The bitrate.</param>
    /// <returns>The formatted bitrate.</returns>
    public static string FormatBitrate(long bitrate)
        => bitrate >= 1_000_000
            ? string.Create(CultureInfo.CurrentCulture, $"{bitrate / 1_000_000.0:0.#} Mbps")
            : string.Create(CultureInfo.CurrentCulture, $"{bitrate / 1000} kbps");
}
