using Jellyfin.Maui.ContentViews;

namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// Server select template.
/// </summary>
public class UserSelectTemplate : DataTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSelectTemplate"/> class.
    /// </summary>
    public UserSelectTemplate()
        : base(typeof(UserSelectView))
    {
    }
}
