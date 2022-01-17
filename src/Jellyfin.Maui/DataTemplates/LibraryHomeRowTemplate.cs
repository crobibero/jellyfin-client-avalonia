using Jellyfin.Maui.ContentViews;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// DataTemplate for home row of libraries.
/// </summary>
public class LibraryHomeRowTemplate : DataTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryHomeRowTemplate"/> class.
    /// </summary>
    public LibraryHomeRowTemplate()
        : base(typeof(HomeRowView))
    {
    }
}
