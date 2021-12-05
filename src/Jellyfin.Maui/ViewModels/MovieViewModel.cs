using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Movie view model.
/// </summary>
public class MovieViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MovieViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    public MovieViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
