using System.Collections.ObjectModel;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Season view model.
/// </summary>
public class SeriesViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    private BaseItemDto? _nextUpItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public SeriesViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;

        BindingBase.EnableCollectionSynchronization(SeasonsCollection, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets the list of seasons.
    /// </summary>
    public ObservableCollection<BaseItemDto> SeasonsCollection { get; } = new();

    /// <summary>
    /// Gets or sets the next up item.
    /// </summary>
    public BaseItemDto? NextUpItem
    {
        get => _nextUpItem;
        set => SetProperty(ref _nextUpItem, value);
    }

    /// <inheritdoc/>
    public override ValueTask InitializeAsync()
    {
        GetSeriesAsync().SafeFireAndForget();
        GetNextUpAsync().SafeFireAndForget();
        GetSeasonsAsync().SafeFireAndForget();

        return ValueTask.CompletedTask;
    }

    private async ValueTask GetSeriesAsync()
    {
        Item = await _libraryService.GetItemAsync(Id).ConfigureAwait(false);
    }

    private async ValueTask GetNextUpAsync()
    {
        var nextUp = await _libraryService.GetNextUpAsync(Id).ConfigureAwait(false);
        if (nextUp.Items.Count > 0)
        {
            NextUpItem = nextUp.Items[0];
        }
    }

    private async ValueTask GetSeasonsAsync()
    {
        var seasonResult = await _libraryService.GetSeasonsAsync(Id).ConfigureAwait(false);
        SeasonsCollection.Clear();
        foreach (var season in seasonResult.Items)
        {
            SeasonsCollection.Add(season);
        }
    }
}
