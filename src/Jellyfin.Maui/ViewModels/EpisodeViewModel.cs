using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Episode view model.
/// </summary>
public class EpisodeViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    public EpisodeViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
