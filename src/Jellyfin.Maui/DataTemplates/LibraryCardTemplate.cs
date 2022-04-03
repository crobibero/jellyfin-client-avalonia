using Jellyfin.Maui.ContentViews;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// DataTemplate for BaseItem.
/// </summary>
public class LibraryCardTemplate : DataTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryCardTemplate"/> class.
    /// </summary>
    public LibraryCardTemplate()
        : base(typeof(LibraryCardView))
    {
    }
}
