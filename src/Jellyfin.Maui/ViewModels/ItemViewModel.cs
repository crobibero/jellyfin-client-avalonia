using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Item view model.
/// </summary>
public class ItemViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/>.</param>
    public ItemViewModel(ILibraryService libraryService)
        : base(libraryService)
    {
    }
}
