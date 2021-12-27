using Jellyfin.Maui.ContentViews;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// Server select template.
/// </summary>
public class ServerSelectTemplate : DataTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectTemplate"/> class.
    /// </summary>
    public ServerSelectTemplate()
        : base(typeof(ServerSelectView))
    {
    }
}
