using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Movie view model.
/// </summary>
public class MovieViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovieViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public MovieViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;
    }

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        Item = await _libraryService.GetItemAsync(Id, ViewModelCancellationToken)
            .ConfigureAwait(false);
    }
}
