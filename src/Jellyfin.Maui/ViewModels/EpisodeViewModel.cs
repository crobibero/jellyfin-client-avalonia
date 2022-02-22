using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Episode view model.
/// </summary>
public class EpisodeViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigation"/> interface.</param>
    public EpisodeViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;
    }

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        Item = await _libraryService.GetItemAsync(Id)
            .ConfigureAwait(false);
    }
}
