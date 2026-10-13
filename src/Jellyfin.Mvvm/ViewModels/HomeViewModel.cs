using System.Globalization;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.Logging;
using HomeSection = Jellyfin.Mvvm.Models.DisplayPreferencesModel.HomeSection;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public sealed partial class HomeViewModel : BrowseViewModel, IDisposable
{
    private static readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30);

    private static readonly BaseItemDto_CollectionType[] _latestExcludedTypes =
    [
        BaseItemDto_CollectionType.Playlists,
        BaseItemDto_CollectionType.Livetv,
        BaseItemDto_CollectionType.Boxsets,
    ];

    private readonly ILibraryService _libraryService;
    private readonly IStateService _stateService;
    private readonly IServerEventService _serverEventService;
    private readonly ILogger<HomeViewModel> _logger;
    private readonly List<(HomeRowModel Row, Func<Task<IReadOnlyList<ItemCardModel>>> Loader, bool Refresh)> _rowLoaders = [];
    private DateTime _lastRefresh;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="backdropService">The backdrop service.</param>
    /// <param name="cardFactory">The card factory.</param>
    /// <param name="serverEventService">The server event service.</param>
    /// <param name="logger">The logger.</param>
    public HomeViewModel(
        ILibraryService libraryService,
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateService stateService,
        IBackdropService backdropService,
        CardFactory cardFactory,
        IServerEventService serverEventService,
        ILogger<HomeViewModel> logger)
        : base(navigationService, applicationService, backdropService, cardFactory)
    {
        _libraryService = libraryService;
        _stateService = stateService;
        _serverEventService = serverEventService;
        _logger = logger;

        _serverEventService.UserDataChanged += OnUserDataChanged;
        _serverEventService.LibraryChanged += OnLibraryChanged;
    }

    /// <inheritdoc />
    public override ToolbarItem ActiveToolbarItem => ToolbarItem.Home;

    /// <summary>
    /// Gets the home rows.
    /// </summary>
    public ObservableRangeCollection<HomeRowModel> Rows { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _serverEventService.UserDataChanged -= OnUserDataChanged;
        _serverEventService.LibraryChanged -= OnLibraryChanged;
    }

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        _rowLoaders.Clear();

        var displayPreferencesTask = GetDisplayPreferencesAsync();
        var librariesTask = _libraryService.GetLibrariesAsync().AsTask();
        await Task.WhenAll(displayPreferencesTask, librariesTask).ConfigureAwait(true);

        var displayPreferences = await displayPreferencesTask.ConfigureAwait(true);
        var libraries = await librariesTask.ConfigureAwait(true);
        var userConfig = _stateService.GetCurrentUser().Configuration;
        var excludedViews = userConfig?.LatestItemsExcludes?.Where(v => v is not null).Select(v => v!.Value).ToHashSet() ?? [];

        foreach (var section in displayPreferences.HomeSections)
        {
            switch (section)
            {
                case HomeSection.LibraryTiles:
                    AddRow(Strings.Strings.Home_MyMedia, CardShape.Thumb, () => Task.FromResult(CardFactory.Create(libraries, CardShape.Thumb)), refresh: false);
                    break;
                case HomeSection.Resume:
                    AddRow(Strings.Strings.Home_ContinueWatching, CardShape.Thumb, async () => CardFactory.Create(await _libraryService.GetResumeAsync(MediaType.Video).ConfigureAwait(false), CardShape.Thumb));
                    break;
                case HomeSection.ResumeAudio:
                    AddRow(Strings.Strings.Home_ContinueListening, CardShape.Square, async () => CardFactory.Create(await _libraryService.GetResumeAsync(MediaType.Audio).ConfigureAwait(false), CardShape.Square));
                    break;
                case HomeSection.ResumeBook:
                    AddRow(Strings.Strings.Home_ContinueReading, CardShape.Poster, async () => CardFactory.Create(await _libraryService.GetResumeAsync(MediaType.Book).ConfigureAwait(false), CardShape.Poster));
                    break;
                case HomeSection.NextUp:
                    AddRow(Strings.Strings.Home_NextUp, CardShape.Thumb, async () => CardFactory.Create(await _libraryService.GetNextUpAsync().ConfigureAwait(false), CardShape.Thumb));
                    break;
                case HomeSection.LatestMedia:
                    foreach (var library in OrderLibraries(libraries, userConfig?.OrderedViews))
                    {
                        if (library.Id is not { } libraryId
                            || excludedViews.Contains(libraryId)
                            || (library.CollectionType is { } collectionType && _latestExcludedTypes.Contains(collectionType)))
                        {
                            continue;
                        }

                        var shape = CardFactory.GetLibraryShape(library.CollectionType);
                        AddRow(
                            string.Format(CultureInfo.CurrentCulture, Strings.Strings.Home_Latest, library.Name),
                            shape,
                            async () => CardFactory.Create(await _libraryService.GetLatestAsync(libraryId).ConfigureAwait(false), shape));
                    }

                    break;
            }
        }

        Rows.ReplaceRange(_rowLoaders.Select(r => r.Row));
        await LoadRowsAsync(refreshOnly: false).ConfigureAwait(true);
    }

    /// <inheritdoc />
    protected override async ValueTask OnReturnedAsync()
    {
        await base.OnReturnedAsync().ConfigureAwait(true);
        if (DateTime.UtcNow - _lastRefresh > _refreshInterval)
        {
            await LoadRowsAsync(refreshOnly: true).ConfigureAwait(true);
        }
    }

    private void OnUserDataChanged(object? sender, UserDataChangeInfo e)
    {
        CardFactory.UpdateUserData(Rows.SelectMany(r => r.Items), e);

        // Resume and next up depend on played state, reload them when the user comes back.
        _lastRefresh = DateTime.MinValue;
    }

    private void OnLibraryChanged(object? sender, LibraryUpdateInfo e) => _lastRefresh = DateTime.MinValue;

    private static IEnumerable<BaseItemDto> OrderLibraries(IReadOnlyList<BaseItemDto> libraries, List<Guid?>? orderedViews)
    {
        if (orderedViews is null || orderedViews.Count == 0)
        {
            return libraries;
        }

        return libraries.OrderBy(l =>
        {
            var index = orderedViews.IndexOf(l.Id);
            return index < 0 ? int.MaxValue : index;
        });
    }

    private async Task<DisplayPreferencesModel> GetDisplayPreferencesAsync()
    {
        try
        {
            return await _libraryService.GetDisplayPreferencesAsync().ConfigureAwait(false) ?? DisplayPreferencesModel.Default;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Unable to load display preferences, using defaults");
            return DisplayPreferencesModel.Default;
        }
    }

    private void AddRow(string name, CardShape shape, Func<Task<IReadOnlyList<ItemCardModel>>> loader, bool refresh = true)
        => _rowLoaders.Add((new HomeRowModel(name, shape), loader, refresh));

    private async Task LoadRowsAsync(bool refreshOnly)
    {
        _lastRefresh = DateTime.UtcNow;
        var tasks = _rowLoaders
            .Where(r => !refreshOnly || r.Refresh)
            .Select(async r =>
            {
                try
                {
                    var items = await r.Loader().ConfigureAwait(true);
                    r.Row.Items = items;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to load home row {Row}", r.Row.Name);
                }
            });

        await Task.WhenAll(tasks).ConfigureAwait(true);

        if (FocusedCard is null)
        {
            var row = Rows.Where(r => r.HasItems).FirstOrDefault(r => !string.Equals(r.Name, Strings.Strings.Home_MyMedia, StringComparison.Ordinal))
                ?? Rows.FirstOrDefault(r => r.HasItems);
            var first = row?.Items.FirstOrDefault();
            if (first is not null)
            {
                FocusCardCommand.Execute(first);
            }
        }
    }
}
