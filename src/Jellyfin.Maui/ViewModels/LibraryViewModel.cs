using System.Collections.ObjectModel;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Library view model.
/// </summary>
public class LibraryViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    private int _pageSize = 100;
    private int _pageIndex;

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
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    /// <summary>
    /// Gets or sets the page index.
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => SetProperty(ref _pageIndex, value);
    }

    /// <summary>
    /// Gets the list of items.
    /// </summary>
    public ObservableCollection<BaseItemDto> LibraryItemsCollection { get; } = new();

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        Item = await _libraryService.GetLibraryAsync(Id, ViewModelCancellationToken)
            .ConfigureAwait(false);
        await InitializeItemsAsync().ConfigureAwait(false);
    }

    private async ValueTask InitializeItemsAsync()
    {
        if (Item is null)
        {
            return;
        }

        var queryResult = await _libraryService.GetLibraryItemsAsync(
                Item,
                PageSize,
                PageSize * PageIndex,
                ViewModelCancellationToken)
            .ConfigureAwait(false);

        LibraryItemsCollection.Clear();
        foreach (var item in queryResult.Items)
        {
            LibraryItemsCollection.Add(item);
        }
    }
}
