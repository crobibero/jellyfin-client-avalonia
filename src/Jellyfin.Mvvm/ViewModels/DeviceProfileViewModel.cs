using System.Globalization;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Shows the detected device capabilities and the device profile sent to the server.
/// </summary>
public partial class DeviceProfileViewModel : BaseViewModel
{
    private readonly PlaybackManager _playbackManager;
    private readonly IPreferencesService _preferencesService;

    [ObservableProperty]
    private string? _profileJson;

    [ObservableProperty]
    private IReadOnlyList<CapabilityEntry> _capabilities = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceProfileViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="playbackManager">The playback manager.</param>
    /// <param name="preferencesService">The preferences service.</param>
    public DeviceProfileViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        PlaybackManager playbackManager,
        IPreferencesService preferencesService)
        : base(navigationService, applicationService)
    {
        _playbackManager = playbackManager;
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Gets the player backend name.
    /// </summary>
    public string BackendName => _playbackManager.Backend.Name;

    /// <summary>
    /// Build the capability entries for display.
    /// </summary>
    /// <param name="c">The capabilities.</param>
    /// <returns>The entries.</returns>
    public static IReadOnlyList<CapabilityEntry> BuildEntries(MediaCapabilities c)
    {
        ArgumentNullException.ThrowIfNull(c);

        static string Yes(bool value) => value ? Strings.Strings.Settings_Supported : Strings.Strings.Settings_Unsupported;
        static string Res(VideoResolution r) => string.Create(CultureInfo.InvariantCulture, $"{r.Width}x{r.Height}");

        return
        [
            new("H.264", Yes(c.SupportsAvc)),
            new("H.264 level", Level(c.AvcMainLevel / 10.0)),
            new("H.264 High 10", Yes(c.SupportsAvcHigh10)),
            new("H.264 " + Strings.Strings.Settings_MaxResolution, Res(c.AvcMaxResolution)),
            new("HEVC", Yes(c.SupportsHevc)),
            new("HEVC level", Level(c.HevcMainLevel / 30.0)),
            new("HEVC Main 10", Yes(c.SupportsHevcMain10)),
            new("HEVC " + Strings.Strings.Settings_MaxResolution, Res(c.HevcMaxResolution)),
            new("HEVC HDR10", Yes(c.SupportsHevcHdr10)),
            new("HEVC HDR10+", Yes(c.SupportsHevcHdr10Plus)),
            new("HEVC Dolby Vision", Yes(c.SupportsHevcDolbyVision)),
            new("HEVC Dolby Vision EL", Yes(c.SupportsHevcDolbyVisionEl)),
            new("AV1", Yes(c.SupportsAv1)),
            new("AV1 Main 10", Yes(c.SupportsAv1Main10)),
            new("AV1 HDR10", Yes(c.SupportsAv1Hdr10)),
            new("AV1 HDR10+", Yes(c.SupportsAv1Hdr10Plus)),
            new("AV1 Dolby Vision", Yes(c.SupportsAv1DolbyVision)),
            new("AV1 " + Strings.Strings.Settings_MaxResolution, Res(c.Av1MaxResolution)),
            new("VC-1", Yes(c.SupportsVc1)),
            new(Strings.Strings.Settings_DirectPlayVideoCodecs, string.Join(", ", c.DirectPlayVideoCodecs)),
            new(Strings.Strings.Settings_DirectPlayContainers, string.Join(", ", c.DirectPlayVideoContainers)),
            new(Strings.Strings.Settings_AudioCodecs, string.Join(", ", c.DecodableAudioCodecs)),
            new(Strings.Strings.Settings_PassthroughCodecs, c.PassthroughAudioCodecs.Count == 0 ? "-" : string.Join(", ", c.PassthroughAudioCodecs)),
            new("Max audio channels", c.MaxAudioChannels.ToString(CultureInfo.CurrentCulture)),
            new("HLS", string.Join(", ", c.HlsContainers)),
            new("ASS/SSA", Yes(c.SupportsAssSubtitles)),
            new("PGS", Yes(c.SupportsPgsSubtitles)),
        ];

        static string Level(double level) => level <= 0 ? "-" : level.ToString("0.0", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        var preferences = _preferencesService.Playback;
        var capabilities = _playbackManager.Backend.GetCapabilities(preferences);
        Capabilities = BuildEntries(capabilities);

        var profile = DeviceProfileFactory.Create(capabilities, preferences);
        var json = await KiotaJsonSerializer.SerializeAsStringAsync(profile).ConfigureAwait(true);
        ProfileJson = Indent(json);
    }

    private static string Indent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                document.WriteTo(writer);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (JsonException)
        {
            return json;
        }
    }
}

/// <summary>
/// A capability shown in the device profile report.
/// </summary>
/// <param name="Name">The capability name.</param>
/// <param name="Value">The capability value.</param>
public sealed record CapabilityEntry(string Name, string Value);
