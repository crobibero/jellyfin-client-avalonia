using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// Login page.
/// </summary>
public class LoginPage : BaseContentPage<LoginViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="LoginViewModel"/>.</param>
    public LoginPage(LoginViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(30),
                Children =
                {
                    new Label()
                        .FillHorizontal()
                        .Bind(Label.TextProperty, nameof(ViewModel.ServerName)),
                    new VerticalStackLayout
                        {
                            new Label { Text = Strings.Login_Username },
                            new Entry()
                                .FillHorizontal()
                                .Bind(Entry.TextProperty, nameof(ViewModel.Username)),
                        },
                    new VerticalStackLayout
                        {
                            new Label { Text = Strings.Login_Password },
                            new Entry()
                                .FillHorizontal()
                                .Bind(Entry.TextProperty, nameof(ViewModel.Password)),
                        },
                    new HorizontalStackLayout
                        {
                            new CheckBox()
                                .Bind(CheckBox.IsCheckedProperty, nameof(ViewModel.RememberMe)),
                            new Label { Text = Strings.Login_RememberMe }
                        },
                    new VerticalStackLayout
                        {
                            new Button { Text = Strings.Login_LoginButton }
                                .CenterHorizontal()
                                .Bind(Button.CommandProperty, nameof(ViewModel.LoginCommand)),
                        },
                    new VerticalStackLayout
                        {
                            new Button { Text = Strings.Login_QuickConnectButton }
                                .CenterHorizontal()
                                .Bind(Button.CommandProperty, nameof(ViewModel.LoginWithQuickConnectCommand)),
                            new HorizontalStackLayout
                                {
                                    new Label
                                    {
                                        Text = Strings.Login_QuickConnectCode,
                                        Padding = new Thickness(0, 0, 5, 0)
                                    },
                                    new Label()
                                        .Bind(Label.TextProperty, nameof(ViewModel.QuickConnectCode), BindingMode.OneWay),
                                }

                            // TODO .Bind(HorizontalStackLayout.IsVisibleProperty, nameof(ViewModel.QuickConnectCode), converter: new IsNotNullOrEmptyConverter(), mode: BindingMode.OneWay)
                        }
                        .Bind(VerticalStackLayout.IsVisibleProperty, nameof(ViewModel.QuickConnectAvailable)),
                    new VerticalStackLayout
                        {
                            new Label()
                                .CenterHorizontal()
                                .Bind(Label.TextProperty, nameof(ViewModel.ErrorMessage))
                        }
                }
            }
        };
    }
}
