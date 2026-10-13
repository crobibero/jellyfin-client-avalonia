using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Item details view model.
/// </summary>
public sealed partial class ItemDetailsViewModel : BaseViewModel, IDisposable
{
    private static readonly BaseItemDto_Type[] _folderPlayableTypes =
    [
        BaseItemDto_Type.Series,
        BaseItemDto_Type.Season,
        BaseItemDto_Type.MusicAlbum,
        BaseItemDto_Type.MusicArtist,
        BaseItemDto_Type.Playlist,
        BaseItemDto_Type.BoxSet,
    ];

    private readonly ILibraryService _libraryService;
    private readonly IBackdropService _backdropService;
    private readonly CardFactory _cardFactory;
    private readonly INotificationService _notificationService;
    private readonly IServerEventService _serverEventService;
    private readonly ILogger<ItemDetailsViewModel> _logger;

    [ObservableProperty]
    private BaseItemDto? _item;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _subtitle;

    [ObservableProperty]
    private string? _logoUrl;

    [ObservableProperty]
    private string? _posterUrl;

    [ObservableProperty]
    private string? _posterBlurHash;

    [ObservableProperty]
    private CardShape _posterShape;

    [ObservableProperty]
    private string? _tagline;

    [ObservableProperty]
    private string? _overview;

    [ObservableProperty]
    private string? _genres;

    [ObservableProperty]
    private string? _directors;

    [ObservableProperty]
    private string? _writers;

    [ObservableProperty]
    private string? _studios;

    [ObservableProperty]
    private IReadOnlyList<InfoChip> _info = [];

    [ObservableProperty]
    private bool _canPlay;

    [ObservableProperty]
    private bool _canResume;

    [ObservableProperty]
    private string? _resumeText;

    [ObservableProperty]
    private bool _canShuffle;

    [ObservableProperty]
    private bool _hasTrailer;

    [ObservableProperty]
    private bool _isFavorite;

    [ObservableProperty]
    private bool _isPlayed;

    [ObservableProperty]
    private bool _canMarkPlayed;

    [ObservableProperty]
    private bool _canGoToSeries;

    [ObservableProperty]
    private IReadOnlyList<OptionItem<int?>> _audioOptions = [];

    [ObservableProperty]
    private OptionItem<int?>? _selectedAudio;

    [ObservableProperty]
    private IReadOnlyList<OptionItem<int?>> _subtitleOptions = [];

    [ObservableProperty]
    private OptionItem<int?>? _selectedSubtitle;

    [ObservableProperty]
    private IReadOnlyList<OptionItem<string?>> _versionOptions = [];

    [ObservableProperty]
    private OptionItem<string?>? _selectedVersion;

    [ObservableProperty]
    private IReadOnlyList<ItemCardModel> _tracks = [];

    private IReadOnlyList<BaseItemDto> _trailers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDetailsViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="backdropService">The backdrop service.</param>
    /// <param name="cardFactory">The card factory.</param>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="serverEventService">The server event service.</param>
    /// <param name="logger">The logger.</param>
    public ItemDetailsViewModel(
        ILibraryService libraryService,
        INavigationService navigationService,
        IApplicationService applicationService,
        IBackdropService backdropService,
        CardFactory cardFactory,
        INotificationService notificationService,
        IServerEventService serverEventService,
        ILogger<ItemDetailsViewModel> logger)
        : base(navigationService, applicationService)
    {
        _libraryService = libraryService;
        _backdropService = backdropService;
        _cardFactory = cardFactory;
        _notificationService = notificationService;
        _serverEventService = serverEventService;
        _logger = logger;

        _serverEventService.UserDataChanged += OnUserDataChanged;
    }

    /// <inheritdoc />
    public override bool UsesBackdrop => true;

    /// <summary>
    /// Gets the rows shown below the details.
    /// </summary>
    public ObservableRangeCollection<HomeRowModel> Rows { get; } = new();

    /// <summary>
    /// Gets a value indicating whether stream selection is available.
    /// </summary>
    public bool HasStreamSelection => AudioOptions.Count > 1 || SubtitleOptions.Count > 1 || VersionOptions.Count > 1;

    /// <summary>
    /// Gets a value indicating whether the item has tracks.
    /// </summary>
    public bool HasTracks => Tracks.Count > 0;

    /// <summary>
    /// Gets a value indicating whether there are multiple versions.
    /// </summary>
    public bool HasVersionChoice => VersionOptions.Count > 1;

    /// <summary>
    /// Gets a value indicating whether there are multiple audio tracks.
    /// </summary>
    public bool HasAudioChoice => AudioOptions.Count > 1;

    /// <summary>
    /// Gets a value indicating whether there are subtitles.
    /// </summary>
    public bool HasSubtitleChoice => SubtitleOptions.Count > 1;

    /// <summary>
    /// Gets a value indicating whether the play button is shown, as opposed to resume and restart.
    /// </summary>
    public bool ShowPlay => CanPlay && !CanResume;

    private Guid ItemId { get; set; }

    /// <inheritdoc />
    public void Dispose() => _serverEventService.UserDataChanged -= OnUserDataChanged;

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        ItemId = parameter switch
        {
            Guid id => id,
            BaseItemDto { Id: { } id } => id,
            _ => Guid.Empty
        };

        if (ItemId == Guid.Empty)
        {
            return;
        }

        await LoadItemAsync().ConfigureAwait(true);
    }

    /// <inheritdoc />
    protected override async ValueTask OnReturnedAsync()
    {
        _backdropService.SetBackdrop(_cardFactory.ImageUrls.GetBackdropUrl(Item));

        // Playback state may have changed, refresh the item.
        try
        {
            var item = await _libraryService.GetItemAsync(ItemId).ConfigureAwait(true);
            if (item is not null)
            {
                Item = item;
                UpdateUserData(item);
                Info = InfoRowBuilder.Build(item, DateTime.Now);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Unable to refresh item");
        }
    }

    partial void OnAudioOptionsChanged(IReadOnlyList<OptionItem<int?>> value)
    {
        OnPropertyChanged(nameof(HasStreamSelection));
        OnPropertyChanged(nameof(HasAudioChoice));
    }

    partial void OnSubtitleOptionsChanged(IReadOnlyList<OptionItem<int?>> value)
    {
        OnPropertyChanged(nameof(HasStreamSelection));
        OnPropertyChanged(nameof(HasSubtitleChoice));
    }

    partial void OnVersionOptionsChanged(IReadOnlyList<OptionItem<string?>> value)
    {
        OnPropertyChanged(nameof(HasStreamSelection));
        OnPropertyChanged(nameof(HasVersionChoice));
    }

    partial void OnCanPlayChanged(bool value) => OnPropertyChanged(nameof(ShowPlay));

    partial void OnCanResumeChanged(bool value) => OnPropertyChanged(nameof(ShowPlay));

    partial void OnTracksChanged(IReadOnlyList<ItemCardModel> value) => OnPropertyChanged(nameof(HasTracks));

    partial void OnSelectedVersionChanged(OptionItem<string?>? value)
    {
        var source = Item?.MediaSources?.FirstOrDefault(m => m.Id == value?.Value);
        if (source is not null)
        {
            UpdateStreamOptions(source);
        }
    }

    private async Task LoadItemAsync()
    {
        var item = await _libraryService.GetItemAsync(ItemId).ConfigureAwait(true);
        if (item is null)
        {
            return;
        }

        Item = item;
        PopulateDetails(item);
        _backdropService.SetBackdrop(_cardFactory.ImageUrls.GetBackdropUrl(item));

        Rows.Clear();
        await PopulateRowsAsync(item).ConfigureAwait(true);
    }

    private void PopulateDetails(BaseItemDto item)
    {
        Title = item.Type switch
        {
            BaseItemDto_Type.Episode or BaseItemDto_Type.Season => item.SeriesName ?? item.Name,
            _ => item.Name
        };

        Subtitle = item.Type switch
        {
            BaseItemDto_Type.Episode => Formatting.FormatEpisodeTitle(item),
            BaseItemDto_Type.Season => item.Name,
            BaseItemDto_Type.MusicAlbum => item.AlbumArtist,
            BaseItemDto_Type.Audio => item.AlbumArtist ?? item.Album,
            _ => item.OriginalTitle is { } originalTitle && !string.Equals(originalTitle, item.Name, StringComparison.Ordinal) ? originalTitle : null
        };

        LogoUrl = _cardFactory.ImageUrls.GetLogoUrl(item);
        PosterShape = item.Type is BaseItemDto_Type.Episode or BaseItemDto_Type.Video ? CardShape.Thumb : CardFactory.GetDefaultShape(item) == CardShape.Square ? CardShape.Square : CardShape.Poster;
        var poster = PosterShape == CardShape.Thumb
            ? _cardFactory.ImageUrls.GetPrimaryImage(item, 540)
            : _cardFactory.ImageUrls.GetCardImage(item, PosterShape, 720);
        PosterUrl = poster?.Url;
        PosterBlurHash = poster?.BlurHash;
        Tagline = item.Taglines?.FirstOrDefault();
        Overview = item.Overview;
        Genres = item.Genres is { Count: > 0 } genres ? string.Join(" · ", genres) : null;
        Studios = item.Studios is { Count: > 0 } studios ? string.Join(", ", studios.Select(s => s.Name)) : null;
        Directors = JoinPeople(item, BaseItemPerson_Type.Director);
        Writers = JoinPeople(item, BaseItemPerson_Type.Writer);
        Info = InfoRowBuilder.Build(item, DateTime.Now);

        CanPlay = item.MediaType is BaseItemDto_MediaType.Video or BaseItemDto_MediaType.Audio
            || (item.Type is { } type && _folderPlayableTypes.Contains(type) && (item.ChildCount ?? 1) > 0);
        CanShuffle = item.IsFolder == true && CanPlay;
        CanMarkPlayed = item.Type is not BaseItemDto_Type.Person and not BaseItemDto_Type.MusicArtist;
        CanGoToSeries = item.Type is BaseItemDto_Type.Episode or BaseItemDto_Type.Season && item.SeriesId is not null;
        HasTrailer = (item.LocalTrailerCount ?? 0) > 0;
        UpdateUserData(item);

        var mediaSources = item.MediaSources ?? [];
        VersionOptions = mediaSources.Count > 1
            ? mediaSources.Select(m => new OptionItem<string?>(m.Name ?? m.Id ?? string.Empty, m.Id)).ToList()
            : [];
        SelectedVersion = VersionOptions.FirstOrDefault();

        if (mediaSources.Count > 0)
        {
            UpdateStreamOptions(mediaSources[0]);
        }
    }

    private void UpdateUserData(BaseItemDto item)
    {
        IsFavorite = item.UserData?.IsFavorite == true;
        IsPlayed = item.UserData?.Played == true;

        var resumeTicks = item.UserData?.PlaybackPositionTicks ?? 0;
        CanResume = resumeTicks > 0 && item.MediaType == BaseItemDto_MediaType.Video;
        ResumeText = CanResume
            ? string.Format(CultureInfo.CurrentCulture, Strings.Strings.Item_ResumeFrom, Formatting.FormatDuration(TimeSpan.FromTicks(resumeTicks)))
            : null;
    }

    private void OnUserDataChanged(object? sender, UserDataChangeInfo e)
    {
        var userDataByItem = CardFactory.GetUserDataByItem(e);
        if (Item is { } item && userDataByItem.TryGetValue(ItemId, out var userData))
        {
            item.UserData = userData;
            UpdateUserData(item);
        }

        _cardFactory.UpdateUserData(Rows.SelectMany(r => r.Items).Concat(Tracks), userDataByItem);
    }

    private void UpdateStreamOptions(MediaSourceInfo mediaSource)
    {
        var streams = mediaSource.MediaStreams ?? [];

        AudioOptions = streams.Where(s => s.Type == MediaStream_Type.Audio)
            .Select(s => new OptionItem<int?>(s.DisplayTitle ?? s.Language ?? s.Codec ?? "?", s.Index))
            .ToList();
        SelectedAudio = AudioOptions.FirstOrDefault(o => o.Value == mediaSource.DefaultAudioStreamIndex) ?? AudioOptions.FirstOrDefault();

        var subtitles = new List<OptionItem<int?>> { new(Strings.Strings.Item_None, -1) };
        subtitles.AddRange(streams.Where(s => s.Type == MediaStream_Type.Subtitle)
            .Select(s => new OptionItem<int?>(s.DisplayTitle ?? s.Language ?? s.Codec ?? "?", s.Index)));
        SubtitleOptions = subtitles;
        SelectedSubtitle = subtitles.FirstOrDefault(o => o.Value == mediaSource.DefaultSubtitleStreamIndex) ?? subtitles[0];
    }

    private static string? JoinPeople(BaseItemDto item, BaseItemPerson_Type type)
    {
        var people = item.People?.Where(p => p.Type == type).Select(p => p.Name).Distinct().ToList();
        return people is { Count: > 0 } ? string.Join(", ", people) : null;
    }

    private async Task PopulateRowsAsync(BaseItemDto item)
    {
        var id = item.Id!.Value;
        var tasks = new List<Task>();

        switch (item.Type)
        {
            case BaseItemDto_Type.Series:
                tasks.Add(AddRowAsync(Strings.Strings.Item_NextUp, CardShape.Thumb, async () => await _libraryService.GetNextUpAsync(id, 1).ConfigureAwait(true), CardTitleStyle.WithinSeries));
                tasks.Add(AddRowAsync(Strings.Strings.Item_Seasons, CardShape.Poster, async () => await _libraryService.GetSeasonsAsync(id).ConfigureAwait(true), CardTitleStyle.WithinSeries));
                break;
            case BaseItemDto_Type.Season when item.SeriesId is not null:
                tasks.Add(AddRowAsync(Strings.Strings.Item_Episodes, CardShape.Thumb, async () => await _libraryService.GetEpisodesAsync(item.SeriesId.Value, id).ConfigureAwait(true), CardTitleStyle.WithinSeries, preferEpisodeImages: true));
                break;
            case BaseItemDto_Type.Episode when item.SeriesId is not null && item.SeasonId is not null:
                tasks.Add(AddRowAsync(Strings.Strings.Item_Episodes, CardShape.Thumb, async () => await _libraryService.GetEpisodesAsync(item.SeriesId.Value, item.SeasonId.Value).ConfigureAwait(true), CardTitleStyle.WithinSeries, preferEpisodeImages: true));
                break;
            case BaseItemDto_Type.MusicAlbum:
            case BaseItemDto_Type.Playlist:
                var children = await _libraryService.GetItemsAsync(new ItemQuery
                {
                    ParentId = id,
                    Recursive = true,
                    SortBy = item.Type == BaseItemDto_Type.Playlist ? [ItemSortBy.Default] : [ItemSortBy.ParentIndexNumber, ItemSortBy.IndexNumber, ItemSortBy.SortName],
                    Limit = 500,
                }).ConfigureAwait(true);
                Tracks = _cardFactory.Create(children.Items ?? [], CardShape.Square, CardTitleStyle.WithinSeries);
                break;
            case BaseItemDto_Type.MusicArtist:
                tasks.Add(AddRowAsync(Strings.Strings.Item_Albums, CardShape.Square, async () => (await _libraryService.GetItemsAsync(new ItemQuery
                {
                    ArtistIds = [id],
                    IncludeItemTypes = [BaseItemKind.MusicAlbum],
                    SortBy = [ItemSortBy.PremiereDate, ItemSortBy.ProductionYear, ItemSortBy.SortName],
                    SortOrder = SortOrder.Descending,
                }).ConfigureAwait(true)).Items ?? []));
                break;
            case BaseItemDto_Type.BoxSet:
                tasks.Add(AddRowAsync(Strings.Strings.Item_Movies, CardShape.Poster, async () => (await _libraryService.GetItemsAsync(new ItemQuery
                {
                    ParentId = id,
                    Recursive = false,
                    SortBy = [ItemSortBy.PremiereDate, ItemSortBy.ProductionYear, ItemSortBy.SortName],
                }).ConfigureAwait(true)).Items ?? []));
                break;
            case BaseItemDto_Type.Person:
                tasks.Add(AddRowAsync(Strings.Strings.Item_Movies, CardShape.Poster, async () => (await _libraryService.GetItemsAsync(new ItemQuery
                {
                    PersonIds = [id],
                    IncludeItemTypes = [BaseItemKind.Movie],
                    SortBy = [ItemSortBy.PremiereDate, ItemSortBy.ProductionYear, ItemSortBy.SortName],
                    SortOrder = SortOrder.Descending,
                }).ConfigureAwait(true)).Items ?? []));
                tasks.Add(AddRowAsync(Strings.Strings.Item_Series, CardShape.Poster, async () => (await _libraryService.GetItemsAsync(new ItemQuery
                {
                    PersonIds = [id],
                    IncludeItemTypes = [BaseItemKind.Series],
                    SortBy = [ItemSortBy.PremiereDate, ItemSortBy.ProductionYear, ItemSortBy.SortName],
                    SortOrder = SortOrder.Descending,
                }).ConfigureAwait(true)).Items ?? []));
                tasks.Add(AddRowAsync(Strings.Strings.Item_Episodes, CardShape.Thumb, async () => (await _libraryService.GetItemsAsync(new ItemQuery
                {
                    PersonIds = [id],
                    IncludeItemTypes = [BaseItemKind.Episode],
                    SortBy = [ItemSortBy.PremiereDate, ItemSortBy.SortName],
                    SortOrder = SortOrder.Descending,
                    Limit = 50,
                }).ConfigureAwait(true)).Items ?? []));
                break;
        }

        if (item.People is { Count: > 0 } people && item.Type != BaseItemDto_Type.Person)
        {
            var row = new HomeRowModel(Strings.Strings.Item_CastAndCrew, CardShape.Poster)
            {
                Items = people.Where(p => p.Id is not null).Take(40).Select(_cardFactory.Create).ToList(),
            };
            Rows.Add(row);
        }

        if (item.Chapters is { Count: > 1 } chapters && item.MediaType == BaseItemDto_MediaType.Video)
        {
            Rows.Add(new HomeRowModel(Strings.Strings.Item_Chapters, CardShape.Thumb)
            {
                Items = chapters.Select((c, i) => ItemCardModel.FromChapter(item, c, i, _cardFactory.ImageUrls)).ToList(),
            });
        }

        if (item.Type is BaseItemDto_Type.Movie or BaseItemDto_Type.Series or BaseItemDto_Type.Episode or BaseItemDto_Type.Video)
        {
            tasks.Add(AddRowAsync(Strings.Strings.Item_Extras, CardShape.Thumb, async () => await _libraryService.GetSpecialFeaturesAsync(id).ConfigureAwait(true)));
        }

        if (item.Type is BaseItemDto_Type.Movie or BaseItemDto_Type.Series or BaseItemDto_Type.MusicAlbum or BaseItemDto_Type.MusicArtist)
        {
            tasks.Add(AddRowAsync(Strings.Strings.Item_MoreLikeThis, null, async () => await _libraryService.GetSimilarAsync(id).ConfigureAwait(true)));
        }

        if (HasTrailer)
        {
            tasks.Add(LoadTrailersAsync(id));
        }

        await Task.WhenAll(tasks).ConfigureAwait(true);
    }

    private async Task LoadTrailersAsync(Guid id)
    {
        try
        {
            _trailers = await _libraryService.GetLocalTrailersAsync(id).ConfigureAwait(true);
            HasTrailer = _trailers.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Unable to load trailers");
            HasTrailer = false;
        }
    }

    private async Task AddRowAsync(string name, CardShape? shape, Func<Task<IReadOnlyList<BaseItemDto>>> loader, CardTitleStyle titleStyle = CardTitleStyle.Default, bool preferEpisodeImages = false)
    {
        var row = new HomeRowModel(name, shape ?? CardShape.Poster);
        Rows.Add(row);

        try
        {
            var items = await loader().ConfigureAwait(true);
            row.Items = preferEpisodeImages
                ? items.Select(i => ItemCardModel.FromItem(i, CardShape.Thumb, _cardFactory.ImageUrls, new Preferences.UserInterfacePreferences { UseSeriesThumbnails = false }, titleStyle)).ToList()
                : _cardFactory.Create(items, shape, titleStyle);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load row {Row}", name);
        }

        if (!row.HasItems)
        {
            Rows.Remove(row);
        }
    }

    private PlaybackRequest CreateRequest(long? startPositionTicks, bool shuffle = false)
        => new([Item!], 0, startPositionTicks, shuffle)
        {
            MediaSourceId = SelectedVersion?.Value,
            AudioStreamIndex = SelectedAudio?.Value,
            SubtitleStreamIndex = SelectedSubtitle?.Value,
        };

    [RelayCommand]
    private async Task PlayAsync()
    {
        if (Item is null)
        {
            return;
        }

        var resume = CanResume ? Item.UserData?.PlaybackPositionTicks : null;
        await NavigationService.NavigateAsync<PlayerViewModel>(CreateRequest(resume)).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RestartAsync()
    {
        if (Item is not null)
        {
            await NavigationService.NavigateAsync<PlayerViewModel>(CreateRequest(null)).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task ShuffleAsync()
    {
        if (Item is not null)
        {
            await NavigationService.NavigateAsync<PlayerViewModel>(CreateRequest(null, shuffle: true)).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task PlayTrailerAsync()
    {
        if (_trailers.Count > 0)
        {
            await NavigationService.NavigateAsync<PlayerViewModel>(new PlaybackRequest(_trailers, 0, null)).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task PlayTrackAsync(ItemCardModel? track)
    {
        if (track?.Item is null)
        {
            return;
        }

        var items = Tracks.Where(t => t.Item is not null).Select(t => t.Item!).ToList();
        var index = Math.Max(0, items.FindIndex(i => i.Id == track.Item.Id));
        await NavigationService.NavigateAsync<PlayerViewModel>(new PlaybackRequest(items, index, null)).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        if (Item?.Id is not { } id)
        {
            return;
        }

        try
        {
            var userData = await _libraryService.SetFavoriteAsync(id, !IsFavorite).ConfigureAwait(true);
            IsFavorite = userData?.IsFavorite ?? !IsFavorite;
        }
        catch (Exception ex)
        {
            _notificationService.Show(ex.Message, isError: true);
        }
    }

    [RelayCommand]
    private async Task TogglePlayedAsync()
    {
        if (Item?.Id is not { } id)
        {
            return;
        }

        try
        {
            var userData = await _libraryService.SetPlayedAsync(id, !IsPlayed).ConfigureAwait(true);
            IsPlayed = userData?.Played ?? !IsPlayed;
            if (userData is not null)
            {
                Item.UserData = userData;
                UpdateUserData(Item);
            }
        }
        catch (Exception ex)
        {
            _notificationService.Show(ex.Message, isError: true);
        }
    }

    [RelayCommand]
    private async Task GoToSeriesAsync()
    {
        if (Item?.SeriesId is { } seriesId)
        {
            await NavigationService.NavigateAsync<ItemDetailsViewModel>(seriesId).ConfigureAwait(true);
        }
    }
}
