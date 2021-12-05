using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Library view model.
/// </summary>
public class LibraryViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/>.</param>
    public LibraryViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
