using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Search view model.
/// </summary>
public sealed partial class SearchViewModel : BrowseViewModel, IDisposable
{
    private static readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(350);

    private readonly ILibraryService _libraryService;
    private readonly ILogger<SearchViewModel> _logger;
    private CancellationTokenSource? _searchCancellation;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string? _emptyMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">The library service.</param>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="backdropService">The backdrop service.</param>
    /// <param name="cardFactory">The card factory.</param>
    /// <param name="logger">The logger.</param>
    public SearchViewModel(
        ILibraryService libraryService,
        INavigationService navigationService,
        IApplicationService applicationService,
        IBackdropService backdropService,
        CardFactory cardFactory,
        ILogger<SearchViewModel> logger)
        : base(navigationService, applicationService, backdropService, cardFactory)
    {
        _libraryService = libraryService;
        _logger = logger;
    }

    /// <inheritdoc />
    public override ToolbarItem ActiveToolbarItem => ToolbarItem.Search;

    /// <summary>
    /// Gets the result rows.
    /// </summary>
    public ObservableRangeCollection<HomeRowModel> Rows { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
    }

    /// <inheritdoc />
    protected override ValueTask InitializeAsync(object? parameter)
    {
        BackdropService.SetBackdrop(null);
        if (parameter is string query)
        {
            SearchText = query;
        }

        return ValueTask.CompletedTask;
    }

    partial void OnSearchTextChanged(string? value) => SearchAsync(value).SafeFireAndForget();

    private async Task SearchAsync(string? query)
    {
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();
        _searchCancellation = new CancellationTokenSource();
        var cancellationToken = _searchCancellation.Token;

        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            Rows.Clear();
            EmptyMessage = null;
            IsSearching = false;
            return;
        }

        try
        {
            await Task.Delay(_debounce, cancellationToken).ConfigureAwait(true);
            IsSearching = true;

            var term = query.Trim();
            var groups = new (string Name, BaseItemKind? Kind, CardShape Shape)[]
            {
                (Strings.Strings.Search_Movies, BaseItemKind.Movie, CardShape.Poster),
                (Strings.Strings.Search_Series, BaseItemKind.Series, CardShape.Poster),
                (Strings.Strings.Search_Episodes, BaseItemKind.Episode, CardShape.Thumb),
                (Strings.Strings.Search_People, null, CardShape.Poster),
                (Strings.Strings.Search_Albums, BaseItemKind.MusicAlbum, CardShape.Square),
                (Strings.Strings.Search_Songs, BaseItemKind.Audio, CardShape.Square),
                (Strings.Strings.Search_Collections, BaseItemKind.BoxSet, CardShape.Poster),
            };

            var tasks = groups.Select(async group =>
            {
                IReadOnlyList<BaseItemDto> items = group.Kind is { } kind
                    ? (await _libraryService.GetItemsAsync(
                            new ItemQuery
                            {
                                SearchTerm = term,
                                IncludeItemTypes = [kind],
                                Limit = 24,
                                SortBy = [ItemSortBy.SortName],
                            },
                            cancellationToken)
                        .ConfigureAwait(true)).Items ?? []
                    : await _libraryService.SearchPeopleAsync(term, 24, cancellationToken).ConfigureAwait(true);

                return new HomeRowModel(group.Name, group.Shape) { Items = CardFactory.Create(items, group.Shape) };
            }).ToList();

            var rows = await Task.WhenAll(tasks).ConfigureAwait(true);
            cancellationToken.ThrowIfCancellationRequested();

            Rows.ReplaceRange(rows.Where(r => r.HasItems));
            EmptyMessage = Rows.Count == 0
                ? string.Format(CultureInfo.CurrentCulture, Strings.Strings.Search_NoResults, term)
                : null;
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer search.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Search failed");
            EmptyMessage = ex.Message;
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IsSearching = false;
            }
        }
    }
}
