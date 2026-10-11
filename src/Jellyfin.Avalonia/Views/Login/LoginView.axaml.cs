using Jellyfin.Avalonia.Controls;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// The view for <see cref="Jellyfin.Mvvm.ViewModels.Login.LoginViewModel"/>.
/// </summary>
public partial class LoginView : PageView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginView"/> class.
    /// </summary>
    public LoginView()
        => InitializeComponent();
}
