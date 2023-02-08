using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Library view model.
/// </summary>
public partial class LibraryViewModel : BaseItemViewModel
{
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private bool _loading = true;

    [ObservableProperty]
    private int _pageSize = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoForward))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private int _pageIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPaginationAvailable))]
    [NotifyPropertyChangedFor(nameof(CanGoForward))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private int _totalCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    public LibraryViewModel(ILibraryService libraryService, INavigationService navigationService, IApplicationService applicationService)
        : base(navigationService, applicationService)
    {
        _libraryService = libraryService;

        ApplicationService.EnableCollectionSynchronization(LibraryItemsCollection, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets a value indicating whether the pagination is available.
    /// </summary>
    public bool IsPaginationAvailable => TotalCount > PageSize;

    /// <summary>
    /// Gets the previous index.
    /// </summary>
    public int PreviousIndex => Math.Max(0, PageIndex - 1);

    /// <summary>
    /// Gets the next index.
    /// </summary>
    public int NextIndex => Math.Min(TotalCount / PageSize, PageIndex + 1);

    /// <summary>
    /// Gets a value indicating whether there is a page before the current page.
    /// </summary>
    public bool CanGoBack => PageIndex != PreviousIndex;

    /// <summary>
    /// Gets a value indicating whether there is a page after the current page.
    /// </summary>
    public bool CanGoForward => PageIndex != NextIndex;

    /// <summary>
    /// Gets the list of items.
    /// </summary>
    public ObservableRangeCollection<BaseItemDto> LibraryItemsCollection { get; } = new();

    /// <inheritdoc />
    public override ValueTask InitializeAsync() => InitializeItemsAsync();

    private async ValueTask InitializeItemsAsync()
    {
        Item = await _libraryService.GetItemAsync(ItemId)
            .ConfigureAwait(false);

        if (Item is null)
        {
            return;
        }

        Loading = true;
        var queryResult = await _libraryService.GetLibraryItemsAsync(
                Item,
                PageSize,
                PageSize * PageIndex)
            .ConfigureAwait(false);

        TotalCount = queryResult.TotalRecordCount;

        // prevents unnecessary refresh (back navigation)
        if (!LibraryItemsCollection.Select(x => x.Id).SequenceEqual(queryResult.Items.Select(x => x.Id)))
        {
            ApplicationService.DispatchAsync(() =>
            {
                LibraryItemsCollection.ReplaceRange(queryResult.Items);
                Loading = false;
            }).SafeFireAndForget();
        }
        else
        {
            Loading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private async Task PreviousPageAsync()
    {
        PageIndex = PreviousIndex;
        await InitializeItemsAsync().ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    private async Task NextPageAsync()
    {
        PageIndex = NextIndex;
        await InitializeItemsAsync().ConfigureAwait(false);
    }
}
