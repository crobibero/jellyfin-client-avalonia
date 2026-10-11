using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// The player view model.
/// </summary>
public sealed partial class PlayerViewModel : BaseViewModel, IBackHandler, IDisposable
{
    private static readonly TimeSpan _overlayTimeout = TimeSpan.FromSeconds(5);
    private static readonly double[] _qualities = [120, 80, 60, 40, 20, 15, 10, 8, 6, 4, 3, 2, 1.5, 1, 0.72, 0.42];
    private static readonly float[] _speeds = [0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f];

    private readonly IPreferencesService _preferencesService;
    private readonly IImageUrlService _imageUrlService;
    private readonly IBackdropService _backdropService;
    private readonly Timer _overlayTimer;
    private Timer? _nextUpTimer;
    private DateTime _lastActivity = DateTime.UtcNow;
    private bool _isClosing;

    [ObservableProperty]
    private bool _isOverlayVisible = true;

    [ObservableProperty]
    private bool _isSeekPreviewVisible;

    [ObservableProperty]
    private PlayerMenu _openMenu;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _subtitle;

    [ObservableProperty]
    private string? _logoUrl;

    [ObservableProperty]
    private string? _artworkUrl;

    [ObservableProperty]
    private string? _artworkBlurHash;

    [ObservableProperty]
    private string? _backdropUrl;

    [ObservableProperty]
    private bool _isVideo = true;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private bool _isBuffering;

    [ObservableProperty]
    private double _positionSeconds;

    [ObservableProperty]
    private double _durationSeconds;

    [ObservableProperty]
    private string _positionText = "0:00";

    [ObservableProperty]
    private string _durationText = "0:00";

    [ObservableProperty]
    private string? _endsAtText;

    [ObservableProperty]
    private bool _isSkipSegmentVisible;

    [ObservableProperty]
    private string? _skipSegmentText;

    [ObservableProperty]
    private bool _isNextUpVisible;

    [ObservableProperty]
    private string? _nextUpTitle;

    [ObservableProperty]
    private string? _nextUpSubtitle;

    [ObservableProperty]
    private string? _nextUpImageUrl;

    [ObservableProperty]
    private string? _nextUpImageBlurHash;

    [ObservableProperty]
    private string? _nextUpCountdownText;

    [ObservableProperty]
    private bool _nextUpShowsImage;

    [ObservableProperty]
    private string? _playbackInfoText;

    [ObservableProperty]
    private IReadOnlyList<TrackOption> _audioOptions = [];

    [ObservableProperty]
    private IReadOnlyList<TrackOption> _subtitleOptions = [];

    [ObservableProperty]
    private IReadOnlyList<TrackOption> _qualityOptions = [];

    [ObservableProperty]
    private IReadOnlyList<TrackOption> _speedOptions = [];

    [ObservableProperty]
    private ZoomMode _zoomMode;

    private int _nextUpRemaining;
    private bool _isSeeking;
    private DateTime _seekPreviewUntil;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="playbackManager">The playback manager.</param>
    /// <param name="preferencesService">The preferences service.</param>
    /// <param name="imageUrlService">The image url service.</param>
    /// <param name="backdropService">The backdrop service.</param>
    public PlayerViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        PlaybackManager playbackManager,
        IPreferencesService preferencesService,
        IImageUrlService imageUrlService,
        IBackdropService backdropService)
        : base(navigationService, applicationService)
    {
        Manager = playbackManager;
        _preferencesService = preferencesService;
        _imageUrlService = imageUrlService;
        _backdropService = backdropService;
        _zoomMode = preferencesService.Playback.ZoomMode;

        Manager.PropertyChanged += OnManagerPropertyChanged;
        Manager.NextUpRequested += OnNextUpRequested;
        Manager.PlaybackFinished += OnPlaybackFinished;

        _overlayTimer = new Timer(_ => ApplicationService.Post(CheckOverlayTimeout), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        SpeedOptions = _speeds.Select(s => new TrackOption(string.Create(CultureInfo.CurrentCulture, $"{s:0.##}x"), (int)(s * 100), Math.Abs(s - 1f) < 0.01)).ToList();
    }

    /// <summary>
    /// The player menus.
    /// </summary>
    public enum PlayerMenu
    {
        /// <summary>No menu is open.</summary>
        None = 0,

        /// <summary>The audio menu.</summary>
        Audio = 1,

        /// <summary>The subtitle menu.</summary>
        Subtitles = 2,

        /// <summary>The quality menu.</summary>
        Quality = 3,

        /// <summary>The speed menu.</summary>
        Speed = 4,

        /// <summary>The playback info panel.</summary>
        Info = 5
    }

    /// <summary>
    /// Gets the playback manager.
    /// </summary>
    public PlaybackManager Manager { get; }

    /// <inheritdoc />
    public override bool ShowToolbar => false;

    /// <inheritdoc />
    public override bool KeepInBackStack => false;

    /// <summary>
    /// Gets a value indicating whether a menu is open.
    /// </summary>
    public bool IsMenuOpen => OpenMenu != PlayerMenu.None;

    /// <summary>
    /// Gets the options of the open menu.
    /// </summary>
    public IReadOnlyList<TrackOption> MenuOptions => OpenMenu switch
    {
        PlayerMenu.Audio => AudioOptions,
        PlayerMenu.Subtitles => SubtitleOptions,
        PlayerMenu.Quality => QualityOptions,
        PlayerMenu.Speed => SpeedOptions,
        _ => []
    };

    /// <summary>
    /// Gets the title of the open menu.
    /// </summary>
    public string? MenuTitle => OpenMenu switch
    {
        PlayerMenu.Audio => Strings.Strings.Player_Audio,
        PlayerMenu.Subtitles => Strings.Strings.Player_Subtitles,
        PlayerMenu.Quality => Strings.Strings.Player_Quality,
        PlayerMenu.Speed => Strings.Strings.Player_Speed,
        PlayerMenu.Info => Strings.Strings.Player_PlaybackInfo,
        _ => null
    };

    /// <summary>
    /// Gets a value indicating whether the playback info panel is open.
    /// </summary>
    public bool IsInfoOpen => OpenMenu == PlayerMenu.Info;

    /// <summary>
    /// Gets a value indicating whether there are subtitle options.
    /// </summary>
    public bool HasSubtitles => SubtitleOptions.Count > 1;

    /// <summary>
    /// Gets a value indicating whether there are multiple audio options.
    /// </summary>
    public bool HasAudioOptions => AudioOptions.Count > 1;

    /// <summary>
    /// Gets the error message of the manager.
    /// </summary>
    public string? PlaybackError => Manager.ErrorMessage;

    /// <summary>
    /// Gets a value indicating whether playback failed.
    /// </summary>
    public bool HasPlaybackError => !string.IsNullOrEmpty(Manager.ErrorMessage);

    /// <inheritdoc />
    public async Task<bool> HandleBackAsync()
    {
        if (IsMenuOpen)
        {
            OpenMenu = PlayerMenu.None;
            return true;
        }

        if (IsNextUpVisible)
        {
            CancelNextUp();
            return true;
        }

        if (IsOverlayVisible && IsVideo && IsPlaying)
        {
            IsOverlayVisible = false;
            return true;
        }

        await CloseAsync().ConfigureAwait(true);
        return true;
    }

    /// <summary>
    /// Report user activity, showing the overlay.
    /// </summary>
    public void ReportActivity()
    {
        _lastActivity = DateTime.UtcNow;
        IsOverlayVisible = true;
    }

    /// <summary>
    /// Seek while the controls are hidden, showing only a small progress indicator.
    /// </summary>
    /// <param name="forward">Whether to seek forward.</param>
    public void PreviewSeek(bool forward)
    {
        if (forward)
        {
            Manager.SkipForward();
        }
        else
        {
            Manager.SkipBack();
        }

        PositionSeconds = Manager.Position.TotalSeconds;
        _seekPreviewUntil = DateTime.UtcNow.AddSeconds(2);
        IsSeekPreviewVisible = true;
    }

    /// <summary>
    /// Begin a seek gesture; position updates from the player are ignored until it ends.
    /// </summary>
    public void BeginSeek()
    {
        _isSeeking = true;
        ReportActivity();
    }

    /// <summary>
    /// End a seek gesture and seek to the current slider position.
    /// </summary>
    public void EndSeek()
    {
        _isSeeking = false;
        Manager.Seek(TimeSpan.FromSeconds(PositionSeconds));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _overlayTimer.Dispose();
        _nextUpTimer?.Dispose();
        Manager.PropertyChanged -= OnManagerPropertyChanged;
        Manager.NextUpRequested -= OnNextUpRequested;
        Manager.PlaybackFinished -= OnPlaybackFinished;
    }

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        _backdropService.SetBackdrop(null);
        if (parameter is not PlaybackRequest request)
        {
            return;
        }

        UpdateQualityOptions();
        await Manager.StartAsync(request).ConfigureAwait(true);
    }

    partial void OnIsOverlayVisibleChanged(bool value)
    {
        if (value)
        {
            IsSeekPreviewVisible = false;
        }
    }

    partial void OnOpenMenuChanged(PlayerMenu value)
    {
        OnPropertyChanged(nameof(IsMenuOpen));
        OnPropertyChanged(nameof(MenuOptions));
        OnPropertyChanged(nameof(MenuTitle));
        OnPropertyChanged(nameof(IsInfoOpen));
        if (value == PlayerMenu.Info)
        {
            UpdatePlaybackInfo();
        }
    }

    partial void OnAudioOptionsChanged(IReadOnlyList<TrackOption> value) => OnPropertyChanged(nameof(HasAudioOptions));

    partial void OnSubtitleOptionsChanged(IReadOnlyList<TrackOption> value) => OnPropertyChanged(nameof(HasSubtitles));

    partial void OnPositionSecondsChanged(double value)
    {
        PositionText = Formatting.FormatDuration(TimeSpan.FromSeconds(value));
        UpdateEndsAt();
    }

    partial void OnDurationSecondsChanged(double value)
    {
        DurationText = Formatting.FormatDuration(TimeSpan.FromSeconds(value));
        UpdateEndsAt();
    }

    private void UpdateEndsAt()
    {
        var remaining = TimeSpan.FromSeconds(Math.Max(0, DurationSeconds - PositionSeconds));
        EndsAtText = DurationSeconds > 0 ? Formatting.FormatEndsAt(remaining.Ticks, DateTime.Now) : null;
    }

    private void OnManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PlaybackManager.CurrentItem):
                UpdateItem();
                break;
            case nameof(PlaybackManager.State):
                IsPlaying = Manager.State == PlayerState.Playing;
                IsBuffering = Manager.State is PlayerState.Opening or PlayerState.Buffering;
                if (!IsPlaying)
                {
                    IsOverlayVisible = true;
                }

                break;
            case nameof(PlaybackManager.Position):
                if (!_isSeeking)
                {
                    PositionSeconds = Manager.Position.TotalSeconds;
                }

                break;
            case nameof(PlaybackManager.Duration):
                DurationSeconds = Manager.Duration.TotalSeconds;
                break;
            case nameof(PlaybackManager.AudioStreams) or nameof(PlaybackManager.AudioStreamIndex):
                UpdateAudioOptions();
                break;
            case nameof(PlaybackManager.SubtitleStreams) or nameof(PlaybackManager.SubtitleStreamIndex):
                UpdateSubtitleOptions();
                break;
            case nameof(PlaybackManager.CurrentSegment):
                UpdateSegment();
                break;
            case nameof(PlaybackManager.MaxBitrateOverride):
                UpdateQualityOptions();
                break;
            case nameof(PlaybackManager.ErrorMessage):
                OnPropertyChanged(nameof(PlaybackError));
                OnPropertyChanged(nameof(HasPlaybackError));
                break;
            case nameof(PlaybackManager.PlayMethod) or nameof(PlaybackManager.CurrentMediaSource):
                UpdatePlaybackInfo();
                break;
        }
    }

    private void UpdateItem()
    {
        var item = Manager.CurrentItem;
        IsNextUpVisible = false;
        if (item is null)
        {
            return;
        }

        IsVideo = item.MediaType != BaseItemDto_MediaType.Audio;
        Title = item.Type switch
        {
            BaseItemDto_Type.Episode => item.SeriesName ?? item.Name,
            BaseItemDto_Type.Audio => item.Name,
            _ => item.Name
        };
        Subtitle = item.Type switch
        {
            BaseItemDto_Type.Episode => Formatting.FormatEpisodeTitle(item),
            BaseItemDto_Type.Audio => string.Join(", ", item.Artists ?? []) is { Length: > 0 } artists ? artists : item.AlbumArtist,
            _ => item.ProductionYear?.ToString(CultureInfo.CurrentCulture)
        };
        LogoUrl = _imageUrlService.GetLogoUrl(item);
        var artwork = _imageUrlService.GetCardImage(item, IsVideo ? CardShape.Poster : CardShape.Square, 720);
        ArtworkUrl = artwork?.Url;
        ArtworkBlurHash = artwork?.BlurHash;
        BackdropUrl = IsVideo ? null : _imageUrlService.GetBackdropUrl(item);
        DurationSeconds = (item.RunTimeTicks is { } ticks ? TimeSpan.FromTicks(ticks) : TimeSpan.Zero).TotalSeconds;
        PositionSeconds = 0;
        ReportActivity();
    }

    private void UpdateAudioOptions()
    {
        AudioOptions = Manager.AudioStreams
            .Select(s => new TrackOption(s.DisplayTitle ?? s.Language ?? s.Codec ?? "?", s.Index, s.Index == Manager.AudioStreamIndex))
            .ToList();
        if (OpenMenu == PlayerMenu.Audio)
        {
            OnPropertyChanged(nameof(MenuOptions));
        }
    }

    private void UpdateSubtitleOptions()
    {
        var options = new List<TrackOption> { new(Strings.Strings.Player_Off, null, Manager.SubtitleStreamIndex is null or < 0) };
        options.AddRange(Manager.SubtitleStreams.Select(s => new TrackOption(s.DisplayTitle ?? s.Language ?? s.Codec ?? "?", s.Index, s.Index == Manager.SubtitleStreamIndex)));
        SubtitleOptions = options;
        if (OpenMenu == PlayerMenu.Subtitles)
        {
            OnPropertyChanged(nameof(MenuOptions));
        }
    }

    private void UpdateQualityOptions()
    {
        var configured = _preferencesService.Playback.MaxBitrateMbps;
        var current = Manager.MaxBitrateOverride;
        var options = new List<TrackOption>
        {
            new(string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_MaxBitrateAuto, configured), null, current is null),
        };
        options.AddRange(_qualities
            .Where(q => q < configured)
            .Select(q => new TrackOption(
                string.Format(CultureInfo.CurrentCulture, Strings.Strings.Settings_MaxBitrateValue, q),
                (int)(q * 1_000_000),
                current == (int)(q * 1_000_000))));
        QualityOptions = options;
        if (OpenMenu == PlayerMenu.Quality)
        {
            OnPropertyChanged(nameof(MenuOptions));
        }
    }

    private void UpdateSegment()
    {
        var segment = Manager.CurrentSegment;
        IsSkipSegmentVisible = segment is not null;
        SkipSegmentText = segment?.Type switch
        {
            MediaSegmentDto_Type.Intro => Strings.Strings.Player_SkipIntro,
            MediaSegmentDto_Type.Outro => Strings.Strings.Player_SkipOutro,
            MediaSegmentDto_Type.Recap => Strings.Strings.Player_SkipRecap,
            MediaSegmentDto_Type.Preview => Strings.Strings.Player_SkipPreview,
            MediaSegmentDto_Type.Commercial => Strings.Strings.Player_SkipCommercial,
            _ => Strings.Strings.Player_SkipSegment
        };
    }

    private void UpdatePlaybackInfo()
    {
        var source = Manager.CurrentMediaSource;
        if (source is null)
        {
            PlaybackInfoText = null;
            return;
        }

        var lines = new List<string>
        {
            Manager.PlayMethod switch
            {
                PlaybackStartInfo_PlayMethod.DirectPlay => Strings.Strings.Player_PlayMethodDirectPlay,
                PlaybackStartInfo_PlayMethod.DirectStream => Strings.Strings.Player_PlayMethodDirectStream,
                _ => Strings.Strings.Player_PlayMethodTranscode
            },
            string.Create(CultureInfo.CurrentCulture, $"{Strings.Strings.Player_Backend}: {Manager.Backend.Name}"),
        };

        if (!string.IsNullOrEmpty(source.Container))
        {
            lines.Add(string.Create(CultureInfo.CurrentCulture, $"{Strings.Strings.Player_Container}: {source.Container}"));
        }

        if (source.Bitrate is { } bitrate)
        {
            lines.Add(string.Create(CultureInfo.CurrentCulture, $"{Strings.Strings.Player_Bitrate}: {Formatting.FormatBitrate(bitrate)}"));
        }

        var video = source.MediaStreams?.FirstOrDefault(s => s.Type == MediaStream_Type.Video);
        if (video is not null)
        {
            lines.Add(string.Create(CultureInfo.CurrentCulture, $"{Strings.Strings.Player_Video}: {video.DisplayTitle ?? video.Codec}"));
        }

        var audio = Manager.AudioStreams.FirstOrDefault(s => s.Index == Manager.AudioStreamIndex);
        if (audio is not null)
        {
            lines.Add(string.Create(CultureInfo.CurrentCulture, $"{Strings.Strings.Player_Audio}: {audio.DisplayTitle ?? audio.Codec}"));
        }

        if (Manager.PlayMethod == PlaybackStartInfo_PlayMethod.Transcode && !string.IsNullOrEmpty(source.TranscodingUrl))
        {
            var reasons = GetQueryValue(source.TranscodingUrl, "TranscodeReasons");
            if (!string.IsNullOrEmpty(reasons))
            {
                lines.Add(string.Create(CultureInfo.CurrentCulture, $"{Strings.Strings.Player_TranscodeReasons}: {reasons.Replace(",", ", ", StringComparison.Ordinal)}"));
            }
        }

        PlaybackInfoText = string.Join(Environment.NewLine, lines);
    }

    private static string? GetQueryValue(string url, string key)
    {
        var query = url.IndexOf('?', StringComparison.Ordinal);
        if (query < 0)
        {
            return null;
        }

        foreach (var part in url[(query + 1)..].Split('&'))
        {
            var separator = part.IndexOf('=', StringComparison.Ordinal);
            if (separator > 0 && string.Equals(part[..separator], key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(part[(separator + 1)..]);
            }
        }

        return null;
    }

    private void CheckOverlayTimeout()
    {
        if (IsSeekPreviewVisible && DateTime.UtcNow > _seekPreviewUntil)
        {
            IsSeekPreviewVisible = false;
        }

        if (IsOverlayVisible && IsPlaying && IsVideo && !IsMenuOpen && !_isSeeking
            && DateTime.UtcNow - _lastActivity > _overlayTimeout)
        {
            IsOverlayVisible = false;
        }
    }

    private void OnNextUpRequested(object? sender, EventArgs e)
    {
        var next = Manager.NextItem;
        if (next is null)
        {
            return;
        }

        var behavior = _preferencesService.Playback.NextUpBehavior;
        NextUpTitle = next.SeriesName ?? next.Name;
        NextUpSubtitle = Formatting.FormatEpisodeTitle(next);
        NextUpShowsImage = behavior == NextUpBehavior.Extended;
        var nextUpImage = _imageUrlService.GetCardImage(next, CardShape.Thumb, 360, preferSeriesImages: false);
        NextUpImageUrl = nextUpImage?.Url;
        NextUpImageBlurHash = nextUpImage?.BlurHash;
        IsNextUpVisible = true;

        _nextUpRemaining = _preferencesService.Playback.NextUpTimeoutSeconds;
        UpdateNextUpCountdown();
        _nextUpTimer?.Dispose();
        if (_nextUpRemaining > 0)
        {
            _nextUpTimer = new Timer(_ => ApplicationService.Post(OnNextUpTick), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }
    }

    private void OnNextUpTick()
    {
        if (!IsNextUpVisible)
        {
            _nextUpTimer?.Dispose();
            return;
        }

        _nextUpRemaining--;
        UpdateNextUpCountdown();
        if (_nextUpRemaining <= 0)
        {
            PlayNextNow();
        }
    }

    private void UpdateNextUpCountdown()
        => NextUpCountdownText = _nextUpRemaining > 0
            ? string.Format(CultureInfo.CurrentCulture, Strings.Strings.Player_StartingIn, _nextUpRemaining)
            : null;

    private void OnPlaybackFinished(object? sender, EventArgs e)
    {
        if (!_isClosing)
        {
            ApplicationService.Post(() => CloseAsync().SafeFireAndForget());
        }
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        ReportActivity();
        Manager.TogglePause();
    }

    [RelayCommand]
    private void SkipForward()
    {
        ReportActivity();
        Manager.SkipForward();
    }

    [RelayCommand]
    private void SkipBack()
    {
        ReportActivity();
        Manager.SkipBack();
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        ReportActivity();
        await Manager.NextAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task PreviousAsync()
    {
        ReportActivity();
        await Manager.PreviousAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private void SkipSegment()
    {
        Manager.SkipSegment();
    }

    [RelayCommand]
    private void PlayNextNow()
    {
        _nextUpTimer?.Dispose();
        _nextUpTimer = null;
        IsNextUpVisible = false;
        Manager.NextAsync().SafeFireAndForget();
    }

    [RelayCommand]
    private void CancelNextUp()
    {
        _nextUpTimer?.Dispose();
        _nextUpTimer = null;
        IsNextUpVisible = false;
        CloseAsync().SafeFireAndForget();
    }

    [RelayCommand]
    private void ShowMenu(PlayerMenu menu)
    {
        ReportActivity();
        OpenMenu = OpenMenu == menu ? PlayerMenu.None : menu;
    }

    [RelayCommand]
    private void CloseMenu() => OpenMenu = PlayerMenu.None;

    [RelayCommand]
    private async Task SelectOptionAsync(TrackOption? option)
    {
        if (option is null)
        {
            return;
        }

        var menu = OpenMenu;
        OpenMenu = PlayerMenu.None;
        ReportActivity();

        switch (menu)
        {
            case PlayerMenu.Audio when option.Value is { } audioIndex:
                await Manager.SelectAudioStreamAsync(audioIndex).ConfigureAwait(true);
                break;
            case PlayerMenu.Subtitles:
                await Manager.SelectSubtitleStreamAsync(option.Value).ConfigureAwait(true);
                break;
            case PlayerMenu.Quality:
                await Manager.SetMaxBitrateAsync(option.Value).ConfigureAwait(true);
                break;
            case PlayerMenu.Speed when option.Value is { } speed && Manager.Player is not null:
                Manager.Player.Speed = speed / 100f;
                SpeedOptions = SpeedOptions.Select(o => o with { IsSelected = o.Value == speed }).ToList();
                break;
        }
    }

    [RelayCommand]
    private void CycleZoom()
    {
        ReportActivity();
        ZoomMode = ZoomMode switch
        {
            ZoomMode.Fit => ZoomMode.AutoCrop,
            ZoomMode.AutoCrop => ZoomMode.Stretch,
            _ => ZoomMode.Fit
        };
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        await Manager.StopAsync().ConfigureAwait(true);
        await NavigationService.GoBackAsync().ConfigureAwait(true);
    }
}

/// <summary>
/// An option in a player menu.
/// </summary>
/// <param name="Label">The display label.</param>
/// <param name="Value">The option value.</param>
/// <param name="IsSelected">Whether the option is selected.</param>
public sealed record TrackOption(string Label, int? Value, bool IsSelected);
