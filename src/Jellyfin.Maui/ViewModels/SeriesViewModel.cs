using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Series view model.
/// </summary>
public class SeriesViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes an instance of the <see cref="SeriesViewModel"/>.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    public SeriesViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
