using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Playback.Profile;
using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.Services;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.Playback;

/// <summary>
/// Manages the play queue, stream selection and progress reporting.
/// </summary>
/// <remarks>
/// Playback follows the server's stream builder: the device profile built from the platform capabilities is sent with a
/// playback info request, and the server decides whether the media is direct played, direct streamed or transcoded.
/// </remarks>
public sealed partial class PlaybackManager : ObservableObject, IDisposable
{
    private static readonly TimeSpan _progressInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _endTolerance = TimeSpan.FromSeconds(30);

    // Matches supportedFonts in jellyfin-web's htmlVideoPlayer plugin.
    private static readonly HashSet<string> _fontMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/vnd.ms-opentype",
        "application/x-truetype-font",
        "font/otf",
        "font/ttf",
        "font/woff",
        "font/woff2",
    };

    private readonly JellyfinApiClient _apiClient;
    private readonly JellyfinSdkSettings _sdkSettings;
    private readonly IStateService _stateService;
    private readonly IStateStorageService _stateStorageService;
    private readonly ILibraryService _libraryService;
    private readonly IPreferencesService _preferencesService;
    private readonly IPlayerBackend _backend;
    private readonly IApplicationService _applicationService;
    private readonly ILogger<PlaybackManager> _logger;

    private CancellationTokenSource _sessionCancellation = new();
    private CancellationTokenSource? _loadCancellation;
    private Timer? _progressTimer;
    private DateTime _lastProgressReport;
    private TimeSpan _lastSegmentCheckPosition;
    private TimeSpan _lastPlayingPosition;
    private bool _startReported;
    private bool _retriedWithTranscoding;
    private string? _playSessionId;
    private bool _disposed;
    private string? _deviceId;
    private PlaybackRequest? _initialSelection;
    private string? _requestedMediaSourceId;

    [ObservableProperty]
    private IMediaPlayer? _player;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNext))]
    [NotifyPropertyChangedFor(nameof(NextItem))]
    private IReadOnlyList<BaseItemDto> _queue = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNext))]
    [NotifyPropertyChangedFor(nameof(HasPrevious))]
    private int _currentIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextItem))]
    private BaseItemDto? _currentItem;

    [ObservableProperty]
    private MediaSourceInfo? _currentMediaSource;

    [ObservableProperty]
    private PlaybackStartInfo_PlayMethod? _playMethod;

    [ObservableProperty]
    private PlayerState _state;

    [ObservableProperty]
    private TimeSpan _position;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private int? _audioStreamIndex;

    [ObservableProperty]
    private int? _subtitleStreamIndex;

    [ObservableProperty]
    private IReadOnlyList<MediaStream> _audioStreams = [];

    [ObservableProperty]
    private IReadOnlyList<MediaStream> _subtitleStreams = [];

    [ObservableProperty]
    private IReadOnlyList<MediaSegmentDto> _segments = [];

    [ObservableProperty]
    private MediaSegmentDto? _currentSegment;

    [ObservableProperty]
    private int? _maxBitrateOverride;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackManager"/> class.
    /// </summary>
    /// <param name="apiClient">The api client.</param>
    /// <param name="sdkSettings">The sdk settings.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="stateStorageService">The state storage service.</param>
    /// <param name="libraryService">The library service.</param>
    /// <param name="preferencesService">The preferences service.</param>
    /// <param name="backend">The player backend.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="logger">The logger.</param>
    public PlaybackManager(
        JellyfinApiClient apiClient,
        JellyfinSdkSettings sdkSettings,
        IStateService stateService,
        IStateStorageService stateStorageService,
        ILibraryService libraryService,
        IPreferencesService preferencesService,
        IPlayerBackend backend,
        IApplicationService applicationService,
        ILogger<PlaybackManager> logger)
    {
        _apiClient = apiClient;
        _sdkSettings = sdkSettings;
        _stateService = stateService;
        _stateStorageService = stateStorageService;
        _libraryService = libraryService;
        _preferencesService = preferencesService;
        _backend = backend;
        _applicationService = applicationService;
        _logger = logger;
    }

    /// <summary>
    /// Raised when the current item ended and another item is queued.
    /// </summary>
    public event EventHandler? NextUpRequested;

    /// <summary>
    /// Raised when the queue finished or playback was stopped.
    /// </summary>
    public event EventHandler? PlaybackFinished;

    /// <summary>
    /// Gets a value indicating whether there is a next item.
    /// </summary>
    public bool HasNext => CurrentIndex >= 0 && CurrentIndex < Queue.Count - 1;

    /// <summary>
    /// Gets a value indicating whether there is a previous item.
    /// </summary>
    public bool HasPrevious => CurrentIndex > 0;

    /// <summary>
    /// Gets the next queued item.
    /// </summary>
    public BaseItemDto? NextItem => HasNext ? Queue[CurrentIndex + 1] : null;

    /// <summary>
    /// Gets a value indicating whether the current item is video.
    /// </summary>
    public bool IsVideo => CurrentItem?.MediaType is BaseItemDto_MediaType.Video;

    /// <summary>
    /// Gets the player backend.
    /// </summary>
    public IPlayerBackend Backend => _backend;

    /// <summary>
    /// Create the device profile for the current preferences.
    /// </summary>
    /// <returns>The device profile.</returns>
    public DeviceProfile CreateDeviceProfile()
    {
        var preferences = GetEffectivePreferences();
        return DeviceProfileFactory.Create(_backend.GetCapabilities(preferences), preferences);
    }

    /// <summary>
    /// Report the client capabilities to the server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ReportCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.Sessions.Capabilities.Full.PostAsync(
                    new ClientCapabilitiesDto
                    {
                        PlayableMediaTypes = [MediaType.Audio, MediaType.Video],
                        SupportedCommands = [.. RemoteControlService.SupportedCommands],
                        SupportsMediaControl = true,
                        SupportsPersistentIdentifier = true,
                        DeviceProfile = CreateDeviceProfile(),
                    },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to report client capabilities");
        }
    }

    /// <summary>
    /// Start playback.
    /// </summary>
    /// <param name="request">The playback request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartAsync(PlaybackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _sessionCancellation.Cancel();
        _sessionCancellation.Dispose();
        _sessionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationToken = _sessionCancellation.Token;
        ErrorMessage = null;
        MaxBitrateOverride = null;

        try
        {
            await BuildQueueAndPlayAsync(request, cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Playback was stopped while the queue was loading.
        }
    }

    private async Task BuildQueueAndPlayAsync(PlaybackRequest request, CancellationToken cancellationToken)
    {
        var queue = new List<BaseItemDto>();
        var startIndex = 0;
        for (var i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (i == request.StartIndex)
            {
                startIndex = queue.Count;
            }

            var playable = await _libraryService.GetPlayableItemsAsync(item, cancellationToken).ConfigureAwait(true);
            queue.AddRange(playable);
        }

        long? startPosition = request.StartPositionTicks;
        if (request.Shuffle)
        {
            Random.Shared.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(queue));
            startIndex = 0;
            startPosition = null;
        }
        else if (request.Items.Count == 1 && request.Items[0].Type is BaseItemDto_Type.Series or BaseItemDto_Type.Season && queue.Count > 0)
        {
            // Start a series at the next up episode.
            startIndex = await GetResumeIndexAsync(request.Items[0], queue, cancellationToken).ConfigureAwait(true);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (queue.Count == 0)
        {
            ErrorMessage = Strings.Strings.Player_NoCompatibleStream;
            State = PlayerState.Error;
            return;
        }

        Queue = queue;
        _initialSelection = request;
        await PlayIndexAsync(Math.Clamp(startIndex, 0, queue.Count - 1), startPosition).ConfigureAwait(true);
    }

    /// <summary>
    /// Add items to the queue, after the current item or at the end.
    /// </summary>
    /// <param name="items">The items, folders are expanded to their playable items.</param>
    /// <param name="playNext">Whether to add the items after the current item instead of at the end.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task EnqueueAsync(IReadOnlyList<BaseItemDto> items, bool playNext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        var session = _sessionCancellation.Token;
        var playable = new List<BaseItemDto>();
        foreach (var item in items)
        {
            playable.AddRange(await _libraryService.GetPlayableItemsAsync(item, cancellationToken).ConfigureAwait(true));
        }

        // A new session started while the items loaded, the queue is no longer the one they were added to.
        if (session.IsCancellationRequested || playable.Count == 0 || CurrentIndex < 0)
        {
            return;
        }

        var queue = Queue.ToList();
        queue.InsertRange(playNext ? CurrentIndex + 1 : queue.Count, playable);
        Queue = queue;
    }

    /// <summary>
    /// Play the item at the queue index.
    /// </summary>
    /// <param name="index">The queue index.</param>
    /// <param name="startPositionTicks">The start position.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PlayIndexAsync(int index, long? startPositionTicks = null)
    {
        if (index < 0 || index >= Queue.Count)
        {
            return;
        }

        await ReportStoppedAsync().ConfigureAwait(true);
        if (_sessionCancellation.IsCancellationRequested)
        {
            return;
        }

        CurrentIndex = index;
        CurrentItem = Queue[index];
        CurrentMediaSource = null;
        AudioStreamIndex = _initialSelection?.AudioStreamIndex;
        SubtitleStreamIndex = _initialSelection?.SubtitleStreamIndex;
        _requestedMediaSourceId = _initialSelection?.MediaSourceId;
        _initialSelection = null;
        Segments = [];
        CurrentSegment = null;
        _retriedWithTranscoding = false;
        OnPropertyChanged(nameof(IsVideo));

        await LoadCurrentAsync(startPositionTicks, enableDirectPlay: true).ConfigureAwait(true);
        LoadSegmentsAsync(CurrentItem).SafeFireAndForget(ex => _logger.LogWarning(ex, "Unable to load media segments"));
    }

    /// <summary>
    /// Play the next item.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task NextAsync()
    {
        if (HasNext)
        {
            await PlayIndexAsync(CurrentIndex + 1).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Play the previous item, or restart the current item when past the first seconds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PreviousAsync()
    {
        if (Position > TimeSpan.FromSeconds(5) || !HasPrevious)
        {
            Seek(TimeSpan.Zero);
            return;
        }

        await PlayIndexAsync(CurrentIndex - 1).ConfigureAwait(true);
    }

    /// <summary>
    /// Stop playback.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StopAsync()
    {
        _sessionCancellation.Cancel();
        _loadCancellation?.Cancel();
        await ReportStoppedAsync().ConfigureAwait(true);
        DetachPlayer();
        Queue = [];
        CurrentIndex = -1;
        CurrentItem = null;
        CurrentMediaSource = null;
        MaxBitrateOverride = null;
        ErrorMessage = null;
        State = PlayerState.Idle;
        PlaybackFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Toggle between play and pause.
    /// </summary>
    public void TogglePause()
    {
        if (Player is null)
        {
            return;
        }

        if (Player.State == PlayerState.Playing)
        {
            Player.Pause();
        }
        else
        {
            Player.Play();
        }
    }

    /// <summary>
    /// Set the player volume and report it to the server.
    /// </summary>
    /// <param name="volume">The volume, from 0 to 100.</param>
    public void SetVolume(int volume)
    {
        if (Player is null)
        {
            return;
        }

        Player.Volume = Math.Clamp(volume, 0, 100);
        ReportProgressAsync().SafeFireAndForget();
    }

    /// <summary>
    /// Mute or unmute the player and report it to the server.
    /// </summary>
    /// <param name="isMuted">Whether to mute.</param>
    public void SetMuted(bool isMuted)
    {
        if (Player is null)
        {
            return;
        }

        Player.IsMuted = isMuted;
        ReportProgressAsync().SafeFireAndForget();
    }

    /// <summary>
    /// Seek to a position.
    /// </summary>
    /// <param name="position">The position.</param>
    public void Seek(TimeSpan position)
    {
        if (Player is null)
        {
            return;
        }

        if (position < TimeSpan.Zero)
        {
            position = TimeSpan.Zero;
        }

        if (Duration > TimeSpan.Zero && position > Duration)
        {
            position = Duration - TimeSpan.FromSeconds(1);
        }

        _lastSegmentCheckPosition = position;
        Player.Seek(position);
        Position = position;
        ReportProgressAsync().SafeFireAndForget();
    }

    /// <summary>
    /// Skip forward by the configured length.
    /// </summary>
    public void SkipForward() => Seek(Position + TimeSpan.FromSeconds(_preferencesService.Playback.SkipForwardSeconds));

    /// <summary>
    /// Skip back by the configured length.
    /// </summary>
    public void SkipBack() => Seek(Position - TimeSpan.FromSeconds(_preferencesService.Playback.SkipBackSeconds));

    /// <summary>
    /// Skip the current media segment.
    /// </summary>
    public void SkipSegment()
    {
        if (CurrentSegment?.EndTicks is { } end)
        {
            CurrentSegment = null;
            Seek(TimeSpan.FromTicks(end));
        }
    }

    /// <summary>
    /// Select an audio stream.
    /// </summary>
    /// <param name="index">The stream index.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SelectAudioStreamAsync(int index)
    {
        if (CurrentMediaSource is null || AudioStreamIndex == index)
        {
            return;
        }

        AudioStreamIndex = index;

        // A direct played file contains all audio tracks, a transcode only the selected one.
        if (PlayMethod is PlaybackStartInfo_PlayMethod.DirectPlay or PlaybackStartInfo_PlayMethod.DirectStream && Player is not null)
        {
            var position = AudioStreams.Select((s, i) => (s, i)).FirstOrDefault(p => p.s.Index == index).i;
            Player.SelectAudioTrack(position);
            ReportProgressAsync().SafeFireAndForget();
            return;
        }

        await ReloadAtCurrentPositionAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Select a subtitle stream.
    /// </summary>
    /// <param name="index">The stream index, or <c>null</c> to disable subtitles.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SelectSubtitleStreamAsync(int? index)
    {
        if (CurrentMediaSource is null || Player is null)
        {
            return;
        }

        var previous = SubtitleStreams.FirstOrDefault(s => s.Index == SubtitleStreamIndex);
        var stream = index is null ? null : SubtitleStreams.FirstOrDefault(s => s.Index == index);
        SubtitleStreamIndex = stream?.Index ?? -1;

        // Burned in subtitles always require a new transcode.
        if (stream?.DeliveryMethod == MediaStream_DeliveryMethod.Encode || previous?.DeliveryMethod == MediaStream_DeliveryMethod.Encode)
        {
            await ReloadAtCurrentPositionAsync().ConfigureAwait(true);
            return;
        }

        ApplySubtitleSelection(stream);
        ReportProgressAsync().SafeFireAndForget();
    }

    /// <summary>
    /// Set the maximum streaming bitrate for the current session.
    /// </summary>
    /// <param name="bitrate">The bitrate in bits per second, or <c>null</c> for the configured maximum.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetMaxBitrateAsync(int? bitrate)
    {
        if (MaxBitrateOverride == bitrate)
        {
            return;
        }

        MaxBitrateOverride = bitrate;
        if (CurrentItem is not null)
        {
            await ReloadAtCurrentPositionAsync().ConfigureAwait(true);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _sessionCancellation.Cancel();
        _sessionCancellation.Dispose();
        _loadCancellation?.Cancel();
        _loadCancellation?.Dispose();
        _progressTimer?.Dispose();
        DetachPlayer();
    }

    private string DeviceId => _deviceId ?? string.Empty;

    private PlaybackPreferences GetEffectivePreferences()
    {
        var preferences = _preferencesService.Playback;
        if (MaxBitrateOverride is { } bitrate)
        {
            preferences = preferences with { MaxBitrateMbps = bitrate / 1_000_000.0 };
        }

        return preferences;
    }

    private async Task<int> GetResumeIndexAsync(BaseItemDto folder, IReadOnlyList<BaseItemDto> queue, CancellationToken cancellationToken)
    {
        var seriesId = folder.Type == BaseItemDto_Type.Series ? folder.Id : folder.SeriesId;
        if (seriesId is null)
        {
            return 0;
        }

        try
        {
            var nextUp = await _libraryService.GetNextUpAsync(seriesId, 1, cancellationToken).ConfigureAwait(true);
            if (nextUp.Count > 0)
            {
                var index = queue.Select((item, i) => (item, i)).FirstOrDefault(p => p.item.Id == nextUp[0].Id).i;
                return index;
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Unable to determine the next up episode");
        }

        return 0;
    }

    private async Task ReloadAtCurrentPositionAsync()
    {
        await ReportStoppedAsync().ConfigureAwait(true);
        await LoadCurrentAsync(Position.Ticks, enableDirectPlay: !_retriedWithTranscoding).ConfigureAwait(true);
    }

    private async Task LoadCurrentAsync(long? startPositionTicks, bool enableDirectPlay)
    {
        var item = CurrentItem;
        if (item?.Id is null)
        {
            return;
        }

        if (_sessionCancellation.IsCancellationRequested)
        {
            return;
        }

        _loadCancellation?.Cancel();
        _loadCancellation?.Dispose();
        _loadCancellation = CancellationTokenSource.CreateLinkedTokenSource(_sessionCancellation.Token);
        var cancellationToken = _loadCancellation.Token;

        ErrorMessage = null;
        State = PlayerState.Opening;

        try
        {
            _deviceId ??= await _stateStorageService.GetDeviceIdAsync().ConfigureAwait(true);

            var preferences = GetEffectivePreferences();
            var capabilities = _backend.GetCapabilities(preferences);
            var profile = DeviceProfileFactory.Create(capabilities, preferences);

            var userId = _stateService.GetCurrentUser().Id;
            var response = await _apiClient.Items[item.Id.Value].PlaybackInfo.PostAsync(
                    new PlaybackInfoDto
                    {
                        UserId = userId,
                        DeviceProfile = profile,
                        MaxStreamingBitrate = preferences.GetMaxBitrate(),
                        StartTimeTicks = startPositionTicks,
                        AudioStreamIndex = AudioStreamIndex,
                        SubtitleStreamIndex = SubtitleStreamIndex,
                        MediaSourceId = CurrentMediaSource?.Id ?? _requestedMediaSourceId,
                        EnableDirectPlay = enableDirectPlay,
                        EnableDirectStream = enableDirectPlay,
                        EnableTranscoding = true,
                        AllowVideoStreamCopy = true,
                        AllowAudioStreamCopy = true,
                        AutoOpenLiveStream = true,
                    },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(true);

            cancellationToken.ThrowIfCancellationRequested();

            if (response?.ErrorCode is not null)
            {
                throw new InvalidOperationException(response.ErrorCode.Value.ToString());
            }

            var mediaSource = SelectMediaSource(response?.MediaSources);
            if (mediaSource is null)
            {
                throw new InvalidOperationException(Strings.Strings.Player_NoCompatibleStream);
            }

            _playSessionId = response!.PlaySessionId;
            CurrentMediaSource = mediaSource;

            var streams = mediaSource.MediaStreams ?? [];
            AudioStreams = streams.Where(s => s.Type == MediaStream_Type.Audio).ToList();
            SubtitleStreams = streams.Where(s => s.Type == MediaStream_Type.Subtitle).ToList();
            AudioStreamIndex ??= mediaSource.DefaultAudioStreamIndex ?? AudioStreams.FirstOrDefault()?.Index;

            // Null means nothing was chosen yet, -1 means subtitles were turned off.
            SubtitleStreamIndex ??= mediaSource.DefaultSubtitleStreamIndex is { } defaultSubtitle and >= 0 ? defaultSubtitle : -1;

            var (url, playMethod) = GetStreamUrl(item, mediaSource);
            PlayMethod = playMethod;

            var start = startPositionTicks is { } ticks and > 0
                ? TimeSpan.FromTicks(Math.Max(0, ticks - TimeSpan.FromSeconds(preferences.ResumeRewindSeconds).Ticks))
                : TimeSpan.Zero;

            var request = new MediaLoadRequest
            {
                Url = url,
                IsVideo = item.MediaType == BaseItemDto_MediaType.Video,
                IsHls = playMethod == PlaybackStartInfo_PlayMethod.Transcode && string.Equals(mediaSource.TranscodingSubProtocol?.ToString(), "Hls", StringComparison.OrdinalIgnoreCase),
                StartPosition = start,
                Duration = mediaSource.RunTimeTicks is { } runtime ? TimeSpan.FromTicks(runtime) : null,
                Title = item.Name,
            };

            request = ApplyTrackSelection(request, mediaSource, playMethod);

            EnsurePlayer(preferences);
            Duration = request.Duration ?? TimeSpan.Zero;
            Position = start;
            _lastPlayingPosition = start;
            _lastSegmentCheckPosition = start;
            _startReported = false;

            _logger.LogInformation("Playing {Item} using {PlayMethod}", item.Name, playMethod);
            await Player!.LoadAsync(request, cancellationToken).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // A newer load replaced this one.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to start playback of {Item}", item.Name);
            ErrorMessage = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Player_Error, ex.Message);
            State = PlayerState.Error;
        }
    }

    private MediaSourceInfo? SelectMediaSource(List<MediaSourceInfo>? mediaSources)
    {
        if (mediaSources is null || mediaSources.Count == 0)
        {
            return null;
        }

        if ((CurrentMediaSource?.Id ?? _requestedMediaSourceId) is { } id)
        {
            var match = mediaSources.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.Ordinal));
            if (match is not null)
            {
                return match;
            }
        }

        return mediaSources[0];
    }

    private (Uri Url, PlaybackStartInfo_PlayMethod PlayMethod) GetStreamUrl(BaseItemDto item, MediaSourceInfo mediaSource)
    {
        var serverUrl = _sdkSettings.ServerUrl?.TrimEnd('/') ?? throw new InvalidOperationException("No server is selected");
        var token = _sdkSettings.AccessToken;

        if (mediaSource.SupportsDirectPlay == true || mediaSource.SupportsDirectStream == true)
        {
            var playMethod = mediaSource.SupportsDirectPlay == true
                ? PlaybackStartInfo_PlayMethod.DirectPlay
                : PlaybackStartInfo_PlayMethod.DirectStream;

            // Remote http sources such as .strm files are played directly.
            if (mediaSource.SupportsDirectPlay == true
                && mediaSource.IsRemote == true
                && mediaSource.Protocol == MediaSourceInfo_Protocol.Http
                && Uri.TryCreate(mediaSource.Path, UriKind.Absolute, out var remote))
            {
                return (remote, playMethod);
            }

            var type = item.MediaType == BaseItemDto_MediaType.Audio ? "Audio" : "Videos";
            var url = string.Create(
                CultureInfo.InvariantCulture,
                $"{serverUrl}/{type}/{item.Id:N}/stream?static=true&mediaSourceId={Uri.EscapeDataString(mediaSource.Id ?? string.Empty)}&playSessionId={Uri.EscapeDataString(_playSessionId ?? string.Empty)}&deviceId={Uri.EscapeDataString(DeviceId)}&ApiKey={Uri.EscapeDataString(token ?? string.Empty)}");
            if (!string.IsNullOrEmpty(mediaSource.ETag))
            {
                url += "&Tag=" + Uri.EscapeDataString(mediaSource.ETag);
            }

            return (new Uri(url), playMethod);
        }

        if (mediaSource.SupportsTranscoding == true && !string.IsNullOrEmpty(mediaSource.TranscodingUrl))
        {
            return (new Uri(serverUrl + mediaSource.TranscodingUrl), PlaybackStartInfo_PlayMethod.Transcode);
        }

        throw new InvalidOperationException(Strings.Strings.Player_NoCompatibleStream);
    }

    private MediaLoadRequest ApplyTrackSelection(MediaLoadRequest request, MediaSourceInfo mediaSource, PlaybackStartInfo_PlayMethod playMethod)
    {
        var isDirect = playMethod is PlaybackStartInfo_PlayMethod.DirectPlay or PlaybackStartInfo_PlayMethod.DirectStream;

        if (isDirect && AudioStreamIndex is { } audioIndex)
        {
            var position = AudioStreams.ToList().FindIndex(s => s.Index == audioIndex);
            if (position >= 0)
            {
                request = request with { AudioTrackPosition = position };
            }
        }

        var subtitle = SubtitleStreams.FirstOrDefault(s => s.Index == SubtitleStreamIndex);
        switch (subtitle?.DeliveryMethod)
        {
            case MediaStream_DeliveryMethod.External when GetDeliveryUri(subtitle) is { } uri:
                request = request with { ExternalSubtitle = CreateExternalSubtitle(uri, subtitle, mediaSource) };
                break;
            case MediaStream_DeliveryMethod.Embed when isDirect:
                request = request with { SubtitleTrackPosition = GetEmbeddedSubtitlePosition(subtitle) };
                break;
            case MediaStream_DeliveryMethod.Hls:
                request = request with { SubtitleTrackPosition = 0 };
                break;
            default:
                request = request with { SubtitleTrackPosition = null };
                break;
        }

        return request;
    }

    private void ApplySubtitleSelection(MediaStream? stream)
    {
        if (Player is null)
        {
            return;
        }

        switch (stream?.DeliveryMethod)
        {
            case null:
                Player.SelectSubtitleTrack(null);
                break;
            case MediaStream_DeliveryMethod.External when GetDeliveryUri(stream) is { } uri:
                Player.LoadExternalSubtitle(CreateExternalSubtitle(uri, stream, CurrentMediaSource));
                break;
            case MediaStream_DeliveryMethod.Embed:
                Player.SelectSubtitleTrack(GetEmbeddedSubtitlePosition(stream));
                break;
            case MediaStream_DeliveryMethod.Hls:
                Player.SelectSubtitleTrack(0);
                break;
        }
    }

    private int GetEmbeddedSubtitlePosition(MediaStream stream)
        => Math.Max(0, SubtitleStreams.Where(s => s.IsExternal != true).ToList().FindIndex(s => s.Index == stream.Index));

    private Uri? GetDeliveryUri(MediaStream stream)
    {
        if (string.IsNullOrEmpty(stream.DeliveryUrl))
        {
            return null;
        }

        if (stream.IsExternalUrl == true && Uri.TryCreate(stream.DeliveryUrl, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        return GetServerUri(stream.DeliveryUrl);
    }

    private Uri? GetServerUri(string? relativeUrl)
    {
        var serverUrl = _sdkSettings.ServerUrl?.TrimEnd('/');
        return serverUrl is null ? null : new Uri(serverUrl + relativeUrl);
    }

    private ExternalSubtitle CreateExternalSubtitle(Uri uri, MediaStream stream, MediaSourceInfo? mediaSource)
    {
        var format = GetSubtitleFormat(stream);
        var subtitle = new ExternalSubtitle(uri, format, stream.Language);
        var isAss = string.Equals(format, Codec.Subtitle.Ass, StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, Codec.Subtitle.Ssa, StringComparison.OrdinalIgnoreCase);
        if (!isAss || mediaSource?.MediaAttachments is not { } attachments)
        {
            return subtitle;
        }

        var fonts = attachments
            .Where(a => a.MimeType is not null && _fontMimeTypes.Contains(a.MimeType) && !string.IsNullOrEmpty(a.DeliveryUrl))
            .Select(a => GetServerUri(a.DeliveryUrl))
            .OfType<Uri>()
            .ToList();
        return subtitle with { Fonts = fonts };
    }

    private static string GetSubtitleFormat(MediaStream stream)
    {
        var url = stream.DeliveryUrl ?? string.Empty;
        var query = url.IndexOf('?', StringComparison.Ordinal);
        var path = query >= 0 ? url[..query] : url;
        var extension = Path.GetExtension(path).TrimStart('.');
        return string.IsNullOrEmpty(extension) ? stream.Codec ?? Codec.Subtitle.Srt : extension;
    }

    private void EnsurePlayer(PlaybackPreferences preferences)
    {
        if (Player is not null)
        {
            return;
        }

        var player = _backend.CreatePlayer(preferences);
        player.StateChanged += OnPlayerStateChanged;
        player.PositionChanged += OnPlayerPositionChanged;
        player.ErrorOccurred += OnPlayerError;
        Player = player;

        _progressTimer = new Timer(_ => ReportProgressAsync().SafeFireAndForget(), null, _progressInterval, _progressInterval);
    }

    private void DetachPlayer()
    {
        _progressTimer?.Dispose();
        _progressTimer = null;

        var player = Player;
        if (player is null)
        {
            return;
        }

        player.StateChanged -= OnPlayerStateChanged;
        player.PositionChanged -= OnPlayerPositionChanged;
        player.ErrorOccurred -= OnPlayerError;
        Player = null;

        try
        {
            player.Stop();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error stopping player");
        }

        player.Dispose();
    }

    private void OnPlayerStateChanged(object? sender, EventArgs e)
    {
        _applicationService.Post(() =>
        {
            if (!ReferenceEquals(sender, Player))
            {
                return;
            }

            var previous = State;
            State = Player!.State;

            if (Player.Duration > TimeSpan.Zero)
            {
                Duration = Player.Duration;
            }

            switch (State)
            {
                case PlayerState.Playing when !_startReported:
                    _startReported = true;
                    ReportStartAsync().SafeFireAndForget();
                    break;
                case PlayerState.Playing or PlayerState.Paused when previous != State:
                    ReportProgressAsync().SafeFireAndForget();
                    break;
                case PlayerState.Ended when previous != PlayerState.Ended:
                    OnEnded();
                    break;
            }
        });
    }

    private void OnPlayerPositionChanged(object? sender, EventArgs e)
    {
        _applicationService.Post(() =>
        {
            if (!ReferenceEquals(sender, Player))
            {
                return;
            }

            Position = Player!.Position;
            if (Player.State != PlayerState.Ended)
            {
                _lastPlayingPosition = Position;
            }

            if (Player.Duration > TimeSpan.Zero)
            {
                Duration = Player.Duration;
            }

            CheckSegments();
        });
    }

    private void OnPlayerError(object? sender, string message)
    {
        _applicationService.Post(() =>
        {
            if (!ReferenceEquals(sender, Player))
            {
                return;
            }

            _logger.LogWarning("Player error: {Message}", message);

            // Retry once with transcoding when direct playback fails.
            if (!_retriedWithTranscoding && PlayMethod is PlaybackStartInfo_PlayMethod.DirectPlay or PlaybackStartInfo_PlayMethod.DirectStream)
            {
                _retriedWithTranscoding = true;
                _logger.LogInformation("Retrying playback with transcoding");
                LoadCurrentAsync(Position.Ticks, enableDirectPlay: false).SafeFireAndForget();
                return;
            }

            ErrorMessage = string.Format(CultureInfo.CurrentCulture, Strings.Strings.Player_Error, message);
            State = PlayerState.Error;
            ReportStoppedAsync(failed: true).SafeFireAndForget();
        });
    }

    private void OnEnded()
    {
        ReportStoppedAsync().SafeFireAndForget();

        if (!HasNext)
        {
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
            return;
        }

        var isEpisode = CurrentItem?.Type == BaseItemDto_Type.Episode;
        if (isEpisode && _preferencesService.Playback.NextUpBehavior != NextUpBehavior.Disabled)
        {
            NextUpRequested?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            NextAsync().SafeFireAndForget();
        }
    }

    private async Task LoadSegmentsAsync(BaseItemDto? item)
    {
        if (item?.Id is null || item.MediaType != BaseItemDto_MediaType.Video)
        {
            return;
        }

        var segments = await _libraryService.GetMediaSegmentsAsync(item.Id.Value).ConfigureAwait(true);
        if (CurrentItem?.Id == item.Id)
        {
            Segments = segments.Where(s => s.StartTicks is not null && s.EndTicks is not null).OrderBy(s => s.StartTicks).ToList();
        }
    }

    private void CheckSegments()
    {
        var position = Position;
        var previous = _lastSegmentCheckPosition;
        _lastSegmentCheckPosition = position;

        MediaSegmentDto? askSegment = null;
        foreach (var segment in Segments)
        {
            var start = TimeSpan.FromTicks(segment.StartTicks!.Value);
            var end = TimeSpan.FromTicks(segment.EndTicks!.Value);
            if (position < start || position >= end)
            {
                continue;
            }

            switch (GetSegmentAction(segment.Type))
            {
                case MediaSegmentAction.Skip:
                    // Only skip when playing into the segment, not when seeking into it.
                    var playedIntoSegment = previous < start && position - previous < TimeSpan.FromSeconds(2);
                    if (playedIntoSegment && end - start >= TimeSpan.FromSeconds(1))
                    {
                        Seek(end);
                        return;
                    }

                    break;
                case MediaSegmentAction.AskToSkip:
                    askSegment = segment;
                    break;
            }
        }

        if (!ReferenceEquals(CurrentSegment, askSegment))
        {
            CurrentSegment = askSegment;
        }
    }

    private MediaSegmentAction GetSegmentAction(MediaSegmentDto_Type? type)
    {
        var preferences = _preferencesService.Playback;
        return type switch
        {
            MediaSegmentDto_Type.Intro => preferences.IntroAction,
            MediaSegmentDto_Type.Outro => preferences.OutroAction,
            MediaSegmentDto_Type.Recap => preferences.RecapAction,
            MediaSegmentDto_Type.Preview => preferences.PreviewAction,
            MediaSegmentDto_Type.Commercial => preferences.CommercialAction,
            _ => MediaSegmentAction.Nothing
        };
    }

    private async Task ReportStartAsync()
    {
        var item = CurrentItem;
        if (item?.Id is null || Player is null)
        {
            return;
        }

        try
        {
            await _apiClient.Sessions.Playing.PostAsync(new PlaybackStartInfo
            {
                ItemId = item.Id,
                MediaSourceId = CurrentMediaSource?.Id,
                PlaySessionId = _playSessionId,
                PlayMethod = PlayMethod,
                AudioStreamIndex = AudioStreamIndex,
                SubtitleStreamIndex = SubtitleStreamIndex ?? -1,
                PositionTicks = Position.Ticks,
                IsPaused = Player.State == PlayerState.Paused,
                IsMuted = Player.IsMuted,
                VolumeLevel = Player.Volume,
                CanSeek = Player.CanSeek,
            }).ConfigureAwait(false);
            _lastProgressReport = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to report playback start");
        }
    }

    private async Task ReportProgressAsync()
    {
        var item = CurrentItem;
        var player = Player;
        if (item?.Id is null || player is null || !_startReported)
        {
            return;
        }

        // Avoid flooding the server with seek reports.
        if (DateTime.UtcNow - _lastProgressReport < TimeSpan.FromSeconds(1))
        {
            return;
        }

        _lastProgressReport = DateTime.UtcNow;

        try
        {
            await _apiClient.Sessions.Playing.Progress.PostAsync(new PlaybackProgressInfo
            {
                ItemId = item.Id,
                MediaSourceId = CurrentMediaSource?.Id,
                PlaySessionId = _playSessionId,
                PlayMethod = PlayMethod switch
                {
                    PlaybackStartInfo_PlayMethod.DirectPlay => PlaybackProgressInfo_PlayMethod.DirectPlay,
                    PlaybackStartInfo_PlayMethod.DirectStream => PlaybackProgressInfo_PlayMethod.DirectStream,
                    _ => PlaybackProgressInfo_PlayMethod.Transcode
                },
                AudioStreamIndex = AudioStreamIndex,
                SubtitleStreamIndex = SubtitleStreamIndex ?? -1,
                PositionTicks = Position.Ticks,
                IsPaused = player.State == PlayerState.Paused,
                IsMuted = player.IsMuted,
                VolumeLevel = player.Volume,
                CanSeek = player.CanSeek,
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to report playback progress");
        }
    }

    private async Task ReportStoppedAsync(bool failed = false)
    {
        var item = CurrentItem;
        if (item?.Id is null || !_startReported)
        {
            return;
        }

        _startReported = false;
        var playSessionId = _playSessionId;
        var position = Position;
        if (State == PlayerState.Ended)
        {
            // A stream can also end early when it is cut off, only report it finished near the real end.
            position = Duration > TimeSpan.Zero && Duration - _lastPlayingPosition <= _endTolerance ? Duration : _lastPlayingPosition;
        }

        try
        {
            await _apiClient.Sessions.Playing.Stopped.PostAsync(new PlaybackStopInfo
            {
                ItemId = item.Id,
                MediaSourceId = CurrentMediaSource?.Id,
                PlaySessionId = playSessionId,
                PositionTicks = position.Ticks,
                Failed = failed,
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to report playback stopped");
        }
    }
}
