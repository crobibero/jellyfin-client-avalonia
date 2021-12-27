using Jellyfin.Maui.ContentViews;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// DataTemplate for BaseItem.
/// </summary>
public class PosterCardTemplate : DataTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PosterCardTemplate"/> class.
    /// </summary>
    public PosterCardTemplate()
        : base(typeof(PosterCardView))
    {
    }
}
