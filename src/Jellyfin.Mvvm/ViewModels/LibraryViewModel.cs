using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Library grid view model.
/// </summary>
public sealed partial class LibraryViewModel : BrowseViewModel, IDisposable
{
    private const int PageSize = 100;

    private readonly ILibraryService _libraryService;
    private CancellationTokenSource? _loadCancellation;
    private bool _isLoadingMore;
    private Guid[]? _randomOrder;
    private int _randomNextIndex;
    private ItemQuery _baseQuery = new();

    [ObservableProperty]
    private BaseItemDto? _library;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private CardShape _shape;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private OptionItem<ItemSortBy[]>? _selectedSort;

    [ObservableProperty]
    private bool _sortDescending;

    [ObservableProperty]
    private OptionItem<LibraryFilter>? _selectedFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="backdropService">The backdrop service.</param>
    /// <param name="cardFactory">The card factory.</param>
    public LibraryViewModel(
        ILibraryService libraryService,
        INavigationService navigationService,
        IApplicationService applicationService,
        IBackdropService backdropService,
        CardFactory cardFactory)
        : base(navigationService, applicationService, backdropService, cardFactory)
    {
        _libraryService = libraryService;

        SortOptions =
        [
            new(Strings.Strings.Library_SortName, [ItemSortBy.SortName]),
            new(Strings.Strings.Library_SortDateAdded, [ItemSortBy.DateCreated, ItemSortBy.SortName]),
            new(Strings.Strings.Library_SortPremiereDate, [ItemSortBy.PremiereDate, ItemSortBy.ProductionYear, ItemSortBy.SortName]),
            new(Strings.Strings.Library_SortRating, [ItemSortBy.CommunityRating, ItemSortBy.SortName]),
            new(Strings.Strings.Library_SortCriticRating, [ItemSortBy.CriticRating, ItemSortBy.SortName]),
            new(Strings.Strings.Library_SortRuntime, [ItemSortBy.Runtime, ItemSortBy.SortName]),
            new(Strings.Strings.Library_SortRandom, [ItemSortBy.Random]),
        ];

        FilterOptions =
        [
            new(Strings.Strings.Library_FilterAll, LibraryFilter.All),
            new(Strings.Strings.Library_FilterUnplayed, LibraryFilter.Unplayed),
            new(Strings.Strings.Library_FilterPlayed, LibraryFilter.Played),
            new(Strings.Strings.Library_FilterFavorites, LibraryFilter.Favorites),
        ];

        _selectedSort = SortOptions[0];
        _selectedFilter = FilterOptions[0];
    }

    /// <summary>
    /// The library filters.
    /// </summary>
    public enum LibraryFilter
    {
        /// <summary>
        /// All items.
        /// </summary>
        All = 0,

        /// <summary>
        /// Unplayed items.
        /// </summary>
        Unplayed = 1,

        /// <summary>
        /// Played items.
        /// </summary>
        Played = 2,

        /// <summary>
        /// Favorite items.
        /// </summary>
        Favorites = 3
    }

    /// <summary>
    /// Gets the sort options.
    /// </summary>
    public IReadOnlyList<OptionItem<ItemSortBy[]>> SortOptions { get; }

    /// <summary>
    /// Gets the filter options.
    /// </summary>
    public IReadOnlyList<OptionItem<LibraryFilter>> FilterOptions { get; }

    /// <summary>
    /// Gets the list of items.
    /// </summary>
    public ObservableRangeCollection<ItemCardModel> Items { get; } = new();

    /// <summary>
    /// Gets a value indicating whether more items can be loaded.
    /// </summary>
    public bool HasMoreItems => Items.Count < TotalCount;

    /// <summary>
    /// Gets the item types shown for a library collection type.
    /// </summary>
    /// <param name="collectionType">The collection type.</param>
    /// <returns>The item types, or <c>null</c> to browse folders.</returns>
    internal static BaseItemKind[]? GetItemTypes(BaseItemDto_CollectionType? collectionType) => collectionType switch
    {
        BaseItemDto_CollectionType.Movies => [BaseItemKind.Movie],
        BaseItemDto_CollectionType.Tvshows => [BaseItemKind.Series],
        BaseItemDto_CollectionType.Music => [BaseItemKind.MusicAlbum],
        BaseItemDto_CollectionType.Musicvideos => [BaseItemKind.MusicVideo],
        BaseItemDto_CollectionType.Boxsets => [BaseItemKind.BoxSet],
        BaseItemDto_CollectionType.Books => [BaseItemKind.Book, BaseItemKind.AudioBook],
        BaseItemDto_CollectionType.Playlists => [BaseItemKind.Playlist],
        BaseItemDto_CollectionType.Trailers => [BaseItemKind.Trailer],
        _ => null
    };

    /// <inheritdoc />
    public void Dispose()
    {
        _loadCancellation?.Cancel();
        _loadCancellation?.Dispose();
    }

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        Library = parameter switch
        {
            BaseItemDto item => item,
            Guid id => await _libraryService.GetItemAsync(id).ConfigureAwait(true),
            _ => null
        };

        if (Library?.Id is not { } libraryId)
        {
            return;
        }

        Title = Library.Name;

        if (Library.Type is BaseItemDto_Type.Genre or BaseItemDto_Type.MusicGenre)
        {
            Shape = Library.Type == BaseItemDto_Type.MusicGenre ? CardShape.Square : CardShape.Poster;
            _baseQuery = new ItemQuery
            {
                GenreIds = [libraryId],
                IncludeItemTypes = Library.Type == BaseItemDto_Type.MusicGenre ? [BaseItemKind.MusicAlbum] : [BaseItemKind.Movie, BaseItemKind.Series],
            };
        }
        else
        {
            var itemTypes = GetItemTypes(Library.CollectionType);
            Shape = CardFactory.GetLibraryShape(Library.CollectionType);
            _baseQuery = new ItemQuery
            {
                ParentId = libraryId,
                IncludeItemTypes = itemTypes,
                Recursive = itemTypes is not null,
            };
        }

        await ReloadAsync().ConfigureAwait(true);
    }

    partial void OnSelectedSortChanged(OptionItem<ItemSortBy[]>? value) => ReloadAsync().SafeFireAndForget();

    partial void OnSortDescendingChanged(bool value) => ReloadAsync().SafeFireAndForget();

    partial void OnSelectedFilterChanged(OptionItem<LibraryFilter>? value) => ReloadAsync().SafeFireAndForget();

    private ItemQuery BuildQuery(int startIndex)
    {
        var filter = SelectedFilter?.Value ?? LibraryFilter.All;
        var sortBy = SelectedSort?.Value ?? [ItemSortBy.SortName];

        // Folders are always listed first when browsing folders.
        if (_baseQuery.Recursive == false)
        {
            sortBy = [ItemSortBy.IsFolder, .. sortBy];
        }

        return _baseQuery with
        {
            SortBy = sortBy,
            SortOrder = SortDescending ? SortOrder.Descending : SortOrder.Ascending,
            IsPlayed = filter switch
            {
                LibraryFilter.Played => true,
                LibraryFilter.Unplayed => false,
                _ => null
            },
            IsFavorite = filter == LibraryFilter.Favorites ? true : null,
            StartIndex = startIndex,
            Limit = PageSize,
        };
    }

    private async Task ReloadAsync()
    {
        if (Library is null)
        {
            return;
        }

        _loadCancellation?.Cancel();
        _loadCancellation?.Dispose();
        _loadCancellation = new CancellationTokenSource();
        var cancellationToken = _loadCancellation.Token;

        Loading = true;
        try
        {
            _randomOrder = null;
            IReadOnlyList<BaseItemDto> items;
            if (SelectedSort?.Value is [ItemSortBy.Random])
            {
                _randomOrder = await GetShuffledIdsAsync(cancellationToken).ConfigureAwait(true);
                _randomNextIndex = 0;
                TotalCount = _randomOrder.Length;
                items = await GetRandomPageAsync(cancellationToken).ConfigureAwait(true);
            }
            else
            {
                var result = await _libraryService.GetItemsAsync(BuildQuery(0), cancellationToken).ConfigureAwait(true);
                TotalCount = result.TotalRecordCount ?? 0;
                items = result.Items ?? [];
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Items.ReplaceRange(CardFactory.Create(items, Shape, CardTitleStyle.Default));
            IsEmpty = Items.Count == 0;
            OnPropertyChanged(nameof(HasMoreItems));
        }
        catch (OperationCanceledException)
        {
            // Replaced by a newer query.
        }
        catch (Exception ex)
        {
            OnError(ex);
        }
        finally
        {
            Loading = false;
        }
    }

    // The server reshuffles a random sort on every request, so paging it by offset repeats and skips items.
    // Shuffle the matching ids once instead and page through them.
    private async Task<Guid[]> GetShuffledIdsAsync(CancellationToken cancellationToken)
    {
        var query = BuildQuery(0) with { SortBy = [ItemSortBy.SortName], StartIndex = null, Limit = null, Fields = [] };
        var result = await _libraryService.GetItemsAsync(query, cancellationToken).ConfigureAwait(true);
        var ids = (result.Items ?? []).Select(i => i.Id).OfType<Guid>().ToArray();
        Random.Shared.Shuffle(ids);
        return ids;
    }

    private async Task<IReadOnlyList<BaseItemDto>> GetRandomPageAsync(CancellationToken cancellationToken)
    {
        var order = _randomOrder!;
        var ids = order.Skip(_randomNextIndex).Take(PageSize).ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        var query = BuildQuery(0) with { SortBy = [ItemSortBy.SortName], Ids = ids, StartIndex = null, Limit = null };
        var result = await _libraryService.GetItemsAsync(query, cancellationToken).ConfigureAwait(true);
        _randomNextIndex += ids.Length;

        // Keep the shuffled order, and drop items removed since the ids were fetched.
        var byId = (result.Items ?? []).Where(i => i.Id is not null).ToDictionary(i => i.Id!.Value);
        var items = ids.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
        TotalCount -= ids.Length - items.Count;
        return items;
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (_isLoadingMore || !HasMoreItems || Library is null)
        {
            return;
        }

        _isLoadingMore = true;
        try
        {
            var cancellationToken = _loadCancellation?.Token ?? CancellationToken.None;
            var items = _randomOrder is null
                ? (await _libraryService.GetItemsAsync(BuildQuery(Items.Count), cancellationToken).ConfigureAwait(true)).Items ?? []
                : await GetRandomPageAsync(cancellationToken).ConfigureAwait(true);
            if (!cancellationToken.IsCancellationRequested && items.Count > 0)
            {
                Items.AddRange(CardFactory.Create(items, Shape, CardTitleStyle.Default));
            }

            OnPropertyChanged(nameof(HasMoreItems));
        }
        catch (OperationCanceledException)
        {
            // Replaced by a newer query.
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    [RelayCommand]
    private void ToggleSortOrder() => SortDescending = !SortDescending;
}
