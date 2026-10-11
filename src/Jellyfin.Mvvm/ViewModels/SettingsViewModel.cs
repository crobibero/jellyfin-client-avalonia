using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Settings view model.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private static readonly double[] _bitrates = [120, 100, 80, 60, 40, 30, 20, 15, 10, 8, 6, 4, 3, 2, 1.5, 1, 0.72, 0.42];
    private static readonly int[] _skipLengths = [5, 10, 15, 20, 30, 45, 60];
    private static readonly int[] _rewindLengths = [0, 3, 5, 10, 15, 30];
    private static readonly int[] _timeouts = [0, 5, 7, 10, 15, 30];

    private readonly IPreferencesService _preferencesService;
    private readonly IStateService _stateService;
    private readonly IDeviceInfo _deviceInfo;
    private readonly Playback.PlaybackManager _playbackManager;
    private bool _isLoading;

    // Playback
    [ObservableProperty]
    private OptionItem<double>? _maxBitrate;

    [ObservableProperty]
    private OptionItem<int>? _resumeRewind;

    [ObservableProperty]
    private OptionItem<int>? _skipBack;

    [ObservableProperty]
    private OptionItem<int>? _skipForward;

    [ObservableProperty]
    private OptionItem<NextUpBehavior>? _nextUpBehavior;

    [ObservableProperty]
    private OptionItem<int>? _nextUpTimeout;

    [ObservableProperty]
    private OptionItem<MediaSegmentAction>? _introAction;

    [ObservableProperty]
    private OptionItem<MediaSegmentAction>? _outroAction;

    [ObservableProperty]
    private OptionItem<MediaSegmentAction>? _recapAction;

    [ObservableProperty]
    private OptionItem<MediaSegmentAction>? _previewAction;

    [ObservableProperty]
    private OptionItem<MediaSegmentAction>? _commercialAction;

    // Audio
    [ObservableProperty]
    private OptionItem<AudioBehavior>? _audioBehavior;

    [ObservableProperty]
    private bool _audioPassthrough;

    [ObservableProperty]
    private OptionItem<BitstreamAudioMode>? _bitstreamAc3;

    [ObservableProperty]
    private OptionItem<BitstreamAudioMode>? _bitstreamEac3;

    [ObservableProperty]
    private OptionItem<BitstreamAudioMode>? _bitstreamDts;

    [ObservableProperty]
    private OptionItem<BitstreamAudioMode>? _bitstreamTrueHd;

    // Video
    [ObservableProperty]
    private bool _hardwareDecoding;

    [ObservableProperty]
    private bool _softwareDecoding;

    [ObservableProperty]
    private OptionItem<int?>? _avcLevel;

    [ObservableProperty]
    private OptionItem<int?>? _hevcLevel;

    [ObservableProperty]
    private OptionItem<HdrOverrideMode>? _hdr10Override;

    [ObservableProperty]
    private OptionItem<HdrOverrideMode>? _hdr10PlusOverride;

    [ObservableProperty]
    private OptionItem<HdrOverrideMode>? _hlgOverride;

    [ObservableProperty]
    private OptionItem<HdrOverrideMode>? _doviProfile5Override;

    [ObservableProperty]
    private OptionItem<HdrOverrideMode>? _doviProfile7Override;

    [ObservableProperty]
    private OptionItem<HdrOverrideMode>? _doviProfile8Override;

    [ObservableProperty]
    private OptionItem<ZoomMode>? _zoomMode;

    // Subtitles
    [ObservableProperty]
    private bool _assDirectPlay;

    [ObservableProperty]
    private bool _pgsDirectPlay;

    // Customization
    [ObservableProperty]
    private bool _backdropEnabled;

    [ObservableProperty]
    private bool _clockEnabled;

    [ObservableProperty]
    private bool _useSeriesThumbnails;

    [ObservableProperty]
    private OptionItem<WatchedIndicatorBehavior>? _watchedIndicator;

    // About
    [ObservableProperty]
    private string? _versionText;

    [ObservableProperty]
    private string? _serverText;

    [ObservableProperty]
    private string? _userText;

    [ObservableProperty]
    private string? _backendText;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="preferencesService">The preferences service.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="deviceInfo">The device info.</param>
    /// <param name="playbackManager">The playback manager.</param>
    public SettingsViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IPreferencesService preferencesService,
        IStateService stateService,
        IDeviceInfo deviceInfo,
        Playback.PlaybackManager playbackManager)
        : base(navigationService, applicationService)
    {
        _preferencesService = preferencesService;
        _stateService = stateService;
        _deviceInfo = deviceInfo;
        _playbackManager = playbackManager;

        BitrateOptions = _bitrates
            .Select(b => new OptionItem<double>(string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_MaxBitrateValue, b), b))
            .ToList();
        SkipOptions = _skipLengths.Select(SecondsOption).ToList();
        RewindOptions = _rewindLengths.Select(SecondsOption).ToList();
        TimeoutOptions = _timeouts.Select(SecondsOption).ToList();
        NextUpOptions =
        [
            new(Strings.Strings.Settings_NextUpExtended, Preferences.NextUpBehavior.Extended),
            new(Strings.Strings.Settings_NextUpMinimal, Preferences.NextUpBehavior.Minimal),
            new(Strings.Strings.Settings_NextUpDisabled, Preferences.NextUpBehavior.Disabled),
        ];
        SegmentOptions =
        [
            new(Strings.Strings.Settings_SegmentNothing, MediaSegmentAction.Nothing),
            new(Strings.Strings.Settings_SegmentAsk, MediaSegmentAction.AskToSkip),
            new(Strings.Strings.Settings_SegmentSkip, MediaSegmentAction.Skip),
        ];
        AudioBehaviorOptions =
        [
            new(Strings.Strings.Settings_AudioDirect, Preferences.AudioBehavior.DirectStream),
            new(Strings.Strings.Settings_AudioDownmix, Preferences.AudioBehavior.DownmixToStereo),
        ];
        BitstreamOptions =
        [
            new(Strings.Strings.Settings_Auto, BitstreamAudioMode.Auto),
            new(Strings.Strings.Settings_Enable, BitstreamAudioMode.Enable),
            new(Strings.Strings.Settings_Disable, BitstreamAudioMode.Disable),
        ];
        HdrOptions =
        [
            new(Strings.Strings.Settings_Auto, HdrOverrideMode.Auto),
            new(Strings.Strings.Settings_Enable, HdrOverrideMode.Enable),
            new(Strings.Strings.Settings_Disable, HdrOverrideMode.Disable),
        ];
        AvcLevelOptions = [new(Strings.Strings.Settings_Auto, null), .. CodecLevels.Avc.Select(l => new OptionItem<int?>(l.Name, l.Level))];
        HevcLevelOptions = [new(Strings.Strings.Settings_Auto, null), .. CodecLevels.Hevc.Select(l => new OptionItem<int?>(l.Name, l.Level))];
        ZoomOptions =
        [
            new(Strings.Strings.Settings_ZoomFit, Preferences.ZoomMode.Fit),
            new(Strings.Strings.Settings_ZoomCrop, Preferences.ZoomMode.AutoCrop),
            new(Strings.Strings.Settings_ZoomStretch, Preferences.ZoomMode.Stretch),
        ];
        WatchedOptions =
        [
            new(Strings.Strings.Settings_WatchedAlways, WatchedIndicatorBehavior.Always),
            new(Strings.Strings.Settings_WatchedHideUnwatched, WatchedIndicatorBehavior.HideUnwatched),
            new(Strings.Strings.Settings_WatchedEpisodesOnly, WatchedIndicatorBehavior.EpisodesOnly),
            new(Strings.Strings.Settings_WatchedNever, WatchedIndicatorBehavior.Never),
        ];
    }

    /// <inheritdoc />
    public override ToolbarItem ActiveToolbarItem => ToolbarItem.Settings;

    /// <summary>Gets the bitrate options.</summary>
    public IReadOnlyList<OptionItem<double>> BitrateOptions { get; }

    /// <summary>Gets the skip length options.</summary>
    public IReadOnlyList<OptionItem<int>> SkipOptions { get; }

    /// <summary>Gets the resume rewind options.</summary>
    public IReadOnlyList<OptionItem<int>> RewindOptions { get; }

    /// <summary>Gets the next up timeout options.</summary>
    public IReadOnlyList<OptionItem<int>> TimeoutOptions { get; }

    /// <summary>Gets the next up behavior options.</summary>
    public IReadOnlyList<OptionItem<NextUpBehavior>> NextUpOptions { get; }

    /// <summary>Gets the media segment action options.</summary>
    public IReadOnlyList<OptionItem<MediaSegmentAction>> SegmentOptions { get; }

    /// <summary>Gets the audio behavior options.</summary>
    public IReadOnlyList<OptionItem<AudioBehavior>> AudioBehaviorOptions { get; }

    /// <summary>Gets the bitstream mode options.</summary>
    public IReadOnlyList<OptionItem<BitstreamAudioMode>> BitstreamOptions { get; }

    /// <summary>Gets the HDR override options.</summary>
    public IReadOnlyList<OptionItem<HdrOverrideMode>> HdrOptions { get; }

    /// <summary>Gets the AVC level options.</summary>
    public IReadOnlyList<OptionItem<int?>> AvcLevelOptions { get; }

    /// <summary>Gets the HEVC level options.</summary>
    public IReadOnlyList<OptionItem<int?>> HevcLevelOptions { get; }

    /// <summary>Gets the zoom options.</summary>
    public IReadOnlyList<OptionItem<ZoomMode>> ZoomOptions { get; }

    /// <summary>Gets the watched indicator options.</summary>
    public IReadOnlyList<OptionItem<WatchedIndicatorBehavior>> WatchedOptions { get; }

    /// <inheritdoc />
    protected override ValueTask InitializeAsync(object? parameter)
    {
        _isLoading = true;
        try
        {
            var p = _preferencesService.Playback;
            MaxBitrate = BitrateOptions.FirstOrDefault(o => Math.Abs(o.Value - p.MaxBitrateMbps) < 0.001) ?? BitrateOptions[1];
            ResumeRewind = Find(RewindOptions, p.ResumeRewindSeconds);
            SkipBack = Find(SkipOptions, p.SkipBackSeconds);
            SkipForward = Find(SkipOptions, p.SkipForwardSeconds);
            NextUpBehavior = Find(NextUpOptions, p.NextUpBehavior);
            NextUpTimeout = Find(TimeoutOptions, p.NextUpTimeoutSeconds);
            IntroAction = Find(SegmentOptions, p.IntroAction);
            OutroAction = Find(SegmentOptions, p.OutroAction);
            RecapAction = Find(SegmentOptions, p.RecapAction);
            PreviewAction = Find(SegmentOptions, p.PreviewAction);
            CommercialAction = Find(SegmentOptions, p.CommercialAction);
            AudioBehavior = Find(AudioBehaviorOptions, p.AudioBehavior);
            AudioPassthrough = p.AudioPassthrough;
            BitstreamAc3 = Find(BitstreamOptions, p.BitstreamAc3);
            BitstreamEac3 = Find(BitstreamOptions, p.BitstreamEac3);
            BitstreamDts = Find(BitstreamOptions, p.BitstreamDts);
            BitstreamTrueHd = Find(BitstreamOptions, p.BitstreamTrueHd);
            HardwareDecoding = p.HardwareDecodingEnabled;
            SoftwareDecoding = p.SoftwareDecodingEnabled;
            AvcLevel = Find(AvcLevelOptions, p.AvcLevel);
            HevcLevel = Find(HevcLevelOptions, p.HevcLevel);
            Hdr10Override = Find(HdrOptions, p.Hdr10Override);
            Hdr10PlusOverride = Find(HdrOptions, p.Hdr10PlusOverride);
            HlgOverride = Find(HdrOptions, p.HlgOverride);
            DoviProfile5Override = Find(HdrOptions, p.DoviProfile5Override);
            DoviProfile7Override = Find(HdrOptions, p.DoviProfile7Override);
            DoviProfile8Override = Find(HdrOptions, p.DoviProfile8Override);
            ZoomMode = Find(ZoomOptions, p.ZoomMode);
            AssDirectPlay = p.AssDirectPlay;
            PgsDirectPlay = p.PgsDirectPlay;

            var ui = _preferencesService.UserInterface;
            BackdropEnabled = ui.BackdropEnabled;
            ClockEnabled = ui.ClockEnabled;
            UseSeriesThumbnails = ui.UseSeriesThumbnails;
            WatchedIndicator = Find(WatchedOptions, ui.WatchedIndicatorBehavior);

            VersionText = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_Version, _deviceInfo.ClientVersion);
            BackendText = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_Backend, _playbackManager.Backend.Name);
            var server = _stateService.GetServerState();
            ServerText = server is null ? null : string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_Server, server.Name, server.Url);
            UserText = _stateService.CurrentUser is { } user ? string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_User, user.Name) : null;
        }
        finally
        {
            _isLoading = false;
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (_isLoading || e.PropertyName is nameof(Loading) or nameof(ErrorMessage) or nameof(HasError)
            or nameof(VersionText) or nameof(ServerText) or nameof(UserText) or nameof(BackendText))
        {
            return;
        }

        SaveAsync().SafeFireAndForget();
    }

    private static OptionItem<int> SecondsOption(int seconds)
        => new(string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_Seconds, seconds), seconds);

    private static OptionItem<T>? Find<T>(IReadOnlyList<OptionItem<T>> options, T value)
        => options.FirstOrDefault(o => EqualityComparer<T>.Default.Equals(o.Value, value)) ?? options.FirstOrDefault();

    private async Task SaveAsync()
    {
        var p = _preferencesService.Playback;
        var playback = p with
        {
            MaxBitrateMbps = MaxBitrate?.Value ?? p.MaxBitrateMbps,
            ResumeRewindSeconds = ResumeRewind?.Value ?? p.ResumeRewindSeconds,
            SkipBackSeconds = SkipBack?.Value ?? p.SkipBackSeconds,
            SkipForwardSeconds = SkipForward?.Value ?? p.SkipForwardSeconds,
            NextUpBehavior = NextUpBehavior?.Value ?? p.NextUpBehavior,
            NextUpTimeoutSeconds = NextUpTimeout?.Value ?? p.NextUpTimeoutSeconds,
            IntroAction = IntroAction?.Value ?? p.IntroAction,
            OutroAction = OutroAction?.Value ?? p.OutroAction,
            RecapAction = RecapAction?.Value ?? p.RecapAction,
            PreviewAction = PreviewAction?.Value ?? p.PreviewAction,
            CommercialAction = CommercialAction?.Value ?? p.CommercialAction,
            AudioBehavior = AudioBehavior?.Value ?? p.AudioBehavior,
            AudioPassthrough = AudioPassthrough,
            BitstreamAc3 = BitstreamAc3?.Value ?? p.BitstreamAc3,
            BitstreamEac3 = BitstreamEac3?.Value ?? p.BitstreamEac3,
            BitstreamDts = BitstreamDts?.Value ?? p.BitstreamDts,
            BitstreamTrueHd = BitstreamTrueHd?.Value ?? p.BitstreamTrueHd,
            HardwareDecodingEnabled = HardwareDecoding,
            SoftwareDecodingEnabled = SoftwareDecoding,
            AvcLevel = AvcLevel is null ? p.AvcLevel : AvcLevel.Value,
            HevcLevel = HevcLevel is null ? p.HevcLevel : HevcLevel.Value,
            Hdr10Override = Hdr10Override?.Value ?? p.Hdr10Override,
            Hdr10PlusOverride = Hdr10PlusOverride?.Value ?? p.Hdr10PlusOverride,
            HlgOverride = HlgOverride?.Value ?? p.HlgOverride,
            DoviProfile5Override = DoviProfile5Override?.Value ?? p.DoviProfile5Override,
            DoviProfile7Override = DoviProfile7Override?.Value ?? p.DoviProfile7Override,
            DoviProfile8Override = DoviProfile8Override?.Value ?? p.DoviProfile8Override,
            ZoomMode = ZoomMode?.Value ?? p.ZoomMode,
            AssDirectPlay = AssDirectPlay,
            PgsDirectPlay = PgsDirectPlay,
        };

        if (playback != p)
        {
            await _preferencesService.SetPlaybackAsync(playback).ConfigureAwait(true);
        }

        var ui = _preferencesService.UserInterface;
        var userInterface = ui with
        {
            BackdropEnabled = BackdropEnabled,
            ClockEnabled = ClockEnabled,
            UseSeriesThumbnails = UseSeriesThumbnails,
            WatchedIndicatorBehavior = WatchedIndicator?.Value ?? ui.WatchedIndicatorBehavior,
        };

        if (userInterface != ui)
        {
            await _preferencesService.SetUserInterfaceAsync(userInterface).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task OpenDeviceProfileAsync()
        => await NavigationService.NavigateAsync<DeviceProfileViewModel>().ConfigureAwait(true);
}
