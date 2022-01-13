using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// The server select view.
/// </summary>
public class UserSelectView : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSelectView"/> class.
    /// </summary>
    public UserSelectView()
    {
        Content = new Label()
            .Bind(Label.TextProperty, mode: BindingMode.OneTime, path: nameof(UserStateModel.Name));
    }
}
