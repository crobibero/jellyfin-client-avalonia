using Jellyfin.Maui.ContentViews.Facades;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// The server select view.
/// </summary>
public class ServerSelectView : BaseContentView<ServerStateModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectView"/> class.
    /// </summary>
    public ServerSelectView()
    {
        Content =
            new Grid
            {
                Children =
                {
                    new Label()
                        .Bind(Label.TextProperty, mode: BindingMode.OneTime, path: nameof(Context.Name))
                }
            }
            .BindClickGesture(
                commandPath: nameof(ServerSelectViewModel.SelectServerCommand),
                commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(ServerSelectViewModel)),
                parameterPath: ".",
                parameterSource: Context)
            .BindTapGesture(
                commandPath: nameof(ServerSelectViewModel.SelectServerCommand),
                commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(ServerSelectViewModel)),
                parameterPath: ".",
                parameterSource: Context);
    }
}
