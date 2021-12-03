using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Season view model.
/// </summary>
public class SeasonViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes an instance of the <see cref="SeasonViewModel"/>.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    public SeasonViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
