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
    private int _pageIndex;

    [ObservableProperty]
    private int _totalCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public LibraryViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;

        BindingBase.EnableCollectionSynchronization(LibraryItemsCollection, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets the list of items.
    /// </summary>
    public ObservableRangeCollection<BaseItemDto> LibraryItemsCollection { get; } = new();

    /// <inheritdoc />
    public override ValueTask InitializeAsync() => InitializeItemsAsync();

    private async ValueTask InitializeItemsAsync()
    {
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
        Application.Current?.Dispatcher.DispatchAsync(() =>
        {
            LibraryItemsCollection.ReplaceRange(queryResult.Items);
            Loading = false;
        }).SafeFireAndForget();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        var newIndex = Math.Max(0, PageIndex - 1);
        if (newIndex == PageIndex)
        {
            return;
        }

        PageIndex = newIndex;
        await InitializeItemsAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        var totalPages = TotalCount / PageSize;
        var newIndex = Math.Min(totalPages, PageIndex + 1);
        if (newIndex == PageIndex)
        {
            return;
        }

        PageIndex = newIndex;
        await InitializeItemsAsync().ConfigureAwait(false);
    }
}
