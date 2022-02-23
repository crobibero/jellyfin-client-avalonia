using Jellyfin.Maui.ContentViews.Facades;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// The server select view.
/// </summary>
public class UserSelectView : BaseContentView<UserStateModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSelectView"/> class.
    /// </summary>
    public UserSelectView()
    {
        Content = new Label()
            .Bind(Label.TextProperty, mode: BindingMode.OneTime, path: nameof(UserStateModel.Name))
            .BindClickGesture(
                commandPath: nameof(SelectUserViewModel.SelectUserCommand),
                commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SelectUserViewModel)),
                parameterPath: ".",
                parameterSource: Context)
            .BindTapGesture(
                commandPath: nameof(SelectUserViewModel.SelectUserCommand),
                commandSource: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestorBindingContext, typeof(SelectUserViewModel)),
                parameterPath: ".",
                parameterSource: Context);
    }
}
