using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Series view model.
/// </summary>
public class SeriesViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    public SeriesViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
