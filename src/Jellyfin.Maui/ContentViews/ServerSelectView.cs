using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// The server select view.
/// </summary>
public class ServerSelectView : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectView"/> class.
    /// </summary>
    public ServerSelectView()
    {
        Content = new Label()
            .Bind(Label.TextProperty, mode: BindingMode.OneTime, path: nameof(ServerStateModel.Name));
    }
}
