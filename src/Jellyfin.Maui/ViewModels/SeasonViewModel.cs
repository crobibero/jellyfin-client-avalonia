using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Season view model.
/// </summary>
public class SeasonViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public SeasonViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;

        BindingBase.EnableCollectionSynchronization(EpisodeCollection, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets the list of episodes.
    /// </summary>
    public ObservableRangeCollection<BaseItemDto> EpisodeCollection { get; } = new();

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        Item = await _libraryService.GetItemAsync(Id).ConfigureAwait(false);
        GetEpisodesAsync().SafeFireAndForget();
    }

    private async ValueTask GetEpisodesAsync()
    {
        if (Item?.SeriesId is null)
        {
            return;
        }

        var episodeResult = await _libraryService.GetEpisodesAsync(Item.SeriesId.Value, Item.Id)
            .ConfigureAwait(false);

        EpisodeCollection.ReplaceRange(episodeResult.Items);
    }
}
