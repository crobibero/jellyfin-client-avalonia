using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Episode view model.
/// </summary>
public class EpisodeViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes an instance of the <see cref="EpisodeViewModel"/>.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    public EpisodeViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
