using CommunityToolkit.Maui.Markup;
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

    private enum Row
    {
        Server,
        Username,
        Password,
        RememberMe,
        LoginButton,
        QuickConnect,
        ErrorMessage
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new ScrollView
        {
            Content = new Grid
            {
                RowSpacing = 25,
                Padding = new Thickness(30),
                RowDefinitions = GridRowsColumns.Rows.Define(
                    (Row.Server, GridRowsColumns.Auto),
                    (Row.Username, GridRowsColumns.Auto),
                    (Row.Password, GridRowsColumns.Auto),
                    (Row.RememberMe, GridRowsColumns.Auto),
                    (Row.LoginButton, GridRowsColumns.Auto),
                    (Row.QuickConnect, GridRowsColumns.Auto),
                    (Row.ErrorMessage, GridRowsColumns.Auto)),
                Children =
                {
                    new Label()
                        .FillExpandHorizontal()
                        .Bind(Label.TextProperty, nameof(ViewModel.ServerName))
                        .Row(Row.Server),
                    new VerticalStackLayout
                        {
                            new Label { Text = Strings.Login_Username },
                            new Entry()
                                .FillExpandHorizontal()
                                .Bind(Entry.TextProperty, nameof(ViewModel.Username)),
                        }
                        .Row(Row.Username),
                    new VerticalStackLayout
                        {
                            new Label { Text = Strings.Login_Password },
                            new Entry()
                                .FillExpandHorizontal()
                                .Bind(Entry.TextProperty, nameof(ViewModel.Password)),
                        }
                        .Row(Row.Password),
                    new HorizontalStackLayout
                        {
                            new CheckBox()
                                .Bind(CheckBox.IsCheckedProperty, nameof(ViewModel.RememberMe)),
                            new Label { Text = Strings.Login_RememberMe }
                        }
                        .Row(Row.RememberMe),
                    new VerticalStackLayout
                        {
                            new Button { Text = Strings.Login_LoginButton }
                                .CenterHorizontal()
                                .Bind(Button.CommandProperty, nameof(ViewModel.LoginCommand)),
                        }
                        .Row(Row.LoginButton),
                    new VerticalStackLayout
                        {
                            new Button { Text = Strings.Login_QuickConnectButton }
                                .CenterHorizontal()
                                .Bind(Button.CommandProperty, nameof(ViewModel.LoginQuickConnectCommand)),
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
                        .Row(Row.QuickConnect)
                        .Bind(VerticalStackLayout.IsVisibleProperty, nameof(ViewModel.QuickConnectAvailable)),
                    new VerticalStackLayout
                        {
                            new Label()
                                .CenterHorizontal()
                                .Bind(Label.TextProperty, nameof(ViewModel.ErrorMessage))
                        }
                        .Row(Row.ErrorMessage)
                }
            }
        };
    }
}
