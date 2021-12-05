using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Item view model.
/// </summary>
public class ItemViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public ItemViewModel(ILibraryService libraryService, INavigationService navigationService)
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
