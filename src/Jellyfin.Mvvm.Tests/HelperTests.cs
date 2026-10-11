using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Mvvm.Tests;

public class HelperTests
{
    [Theory]
    [InlineData("jellyfin.example.com", new[] { "https://jellyfin.example.com", "http://jellyfin.example.com:8096", "https://jellyfin.example.com:8920", "http://jellyfin.example.com" })]
    [InlineData("192.168.1.10:8096", new[] { "https://192.168.1.10:8096", "http://192.168.1.10:8096" })]
    [InlineData("https://demo.jellyfin.org/stable/", new[] { "https://demo.jellyfin.org/stable" })]
    [InlineData("[2001:db8::1]", new[] { "https://[2001:db8::1]", "http://[2001:db8::1]:8096", "https://[2001:db8::1]:8920", "http://[2001:db8::1]" })]
    [InlineData("[2001:db8::1]:8096", new[] { "https://[2001:db8::1]:8096", "http://[2001:db8::1]:8096" })]
    [InlineData("  ", new string[0])]
    public void GetAddressCandidates_ReturnsExpectedUrls(string input, string[] expected)
    {
        Assert.Equal(expected, AuthenticationService.GetAddressCandidates(input));
    }

    [Fact]
    public void DisplayPreferences_DefaultsWhenEmpty()
    {
        var preferences = DisplayPreferencesModel.FromCustomPreferences(new Dictionary<string, object>());
        Assert.Same(DisplayPreferencesModel.Default, preferences);
    }

    [Fact]
    public void DisplayPreferences_ParsesInOrder()
    {
        var preferences = DisplayPreferencesModel.FromCustomPreferences(new Dictionary<string, object>
        {
            ["homesection1"] = "latestmedia",
            ["homesection0"] = "nextup",
            ["homesection2"] = "none",
            ["homesection3"] = "none",
            ["homesection4"] = "none",
            ["somethingelse"] = "resume",
        });

        Assert.Equal(
            [DisplayPreferencesModel.HomeSection.NextUp, DisplayPreferencesModel.HomeSection.LatestMedia],
            preferences.HomeSections);
    }

    [Theory]
    [InlineData(3840, 2160, false, "4K")]
    [InlineData(1920, 800, false, "1080p")]
    [InlineData(1920, 1080, true, "1080i")]
    [InlineData(1280, 720, false, "720p")]
    [InlineData(640, 360, false, "SD")]
    public void GetResolutionName(int width, int height, bool interlaced, string expected)
    {
        Assert.Equal(expected, Formatting.GetResolutionName(width, height, interlaced));
    }

    [Fact]
    public void FormatEpisodeTitle()
    {
        var item = new BaseItemDto { Name = "Pilot", ParentIndexNumber = 1, IndexNumber = 2 };
        Assert.Equal("S1:E2 - Pilot", Formatting.FormatEpisodeTitle(item));
    }

    [Fact]
    public void FormatRuntime()
    {
        Assert.Equal("1h 32m", Formatting.FormatRuntime(TimeSpan.FromMinutes(92).Ticks));
        Assert.Equal("45m", Formatting.FormatRuntime(TimeSpan.FromMinutes(45).Ticks));
        Assert.Null(Formatting.FormatRuntime(null));
    }

    [Fact]
    public void VideoRangeNames_IncludeFallbackLayer()
    {
        Assert.Equal(["Dolby Vision", "HDR10"], InfoRowBuilder.GetVideoRangeNames(MediaStream_VideoRangeType.DOVIWithHDR10));
        Assert.Empty(InfoRowBuilder.GetVideoRangeNames(MediaStream_VideoRangeType.SDR));
    }

    [Fact]
    public void InfoRow_IncludesMediaDetails()
    {
        var item = new BaseItemDto
        {
            Type = BaseItemDto_Type.Movie,
            ProductionYear = 2021,
            OfficialRating = "PG-13",
            CommunityRating = 7.5f,
            MediaStreams =
            [
                new MediaStream { Type = MediaStream_Type.Video, Width = 3840, Height = 2160 },
                new MediaStream { Type = MediaStream_Type.Audio, Codec = "truehd", Profile = "Dolby TrueHD + Dolby Atmos", Channels = 8 },
                new MediaStream { Type = MediaStream_Type.Subtitle, IsHearingImpaired = true },
            ],
        };

        var chips = InfoRowBuilder.Build(item, DateTime.Now).Select(c => c.Text).ToList();

        Assert.Contains("2021", chips);
        Assert.Contains("PG-13", chips);
        Assert.Contains("4K", chips);
        Assert.Contains("TrueHD Atmos", chips);
        Assert.Contains("7.1", chips);
        Assert.Contains("SDH", chips);
    }
}
