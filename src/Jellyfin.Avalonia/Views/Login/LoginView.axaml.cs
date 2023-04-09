using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Views.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// Login view.
/// </summary>
public partial class LoginView : BaseUserView<LoginViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginView"/> class.
    /// </summary>
    public LoginView()
        => AvaloniaXamlLoader.Load(this);
}

