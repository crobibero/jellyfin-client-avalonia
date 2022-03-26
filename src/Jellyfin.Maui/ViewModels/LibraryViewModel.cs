using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Library view model.
/// </summary>
public partial class LibraryViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private int _pageSize = 100;

    [ObservableProperty]
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
    /// Gets the list of items.
    /// </summary>
    public ObservableRangeCollection<BaseItemDto> LibraryItemsCollection { get; } = new();

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        Item = await _libraryService.GetLibraryAsync(Id).ConfigureAwait(false);
        InitializeItemsAsync().SafeFireAndForget();
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
                PageSize * PageIndex)
            .ConfigureAwait(false);

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            LibraryItemsCollection.ReplaceRange(queryResult.Items);
        });
    }
}
