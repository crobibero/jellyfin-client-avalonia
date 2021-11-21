using Jellyfin.Maui.ViewModels;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Login page.
/// </summary>
public class LoginPage : BaseContentPage<LoginViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage"/>.
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
            Content = new Grid
            {
                RowSpacing = 25,
                Padding = Device.RuntimePlatform switch
                {
                    Device.iOS => new Thickness(30, 60, 30, 30),
                    _ => new Thickness(30)
                },
                RowDefinitions = GridRowsColumns.Rows.Define(
                    (Row.Server, GridRowsColumns.Auto),
                    (Row.Username, GridRowsColumns.Auto),
                    (Row.Password, GridRowsColumns.Auto),
                    (Row.LoginButton, GridRowsColumns.Auto),
                    (Row.ErrorMessage, GridRowsColumns.Auto)
                ),
                Children =
                {
                    new VerticalStackLayout
                    {
                        new Label { Text = "Server URL" },
                        new Entry()
                            .FillExpandHorizontal()
                            .Bind(Entry.TextProperty, nameof(ViewModel.ServerUrl)),
                    }.Row(Row.Server),
                    new VerticalStackLayout
                    {
                        new Label { Text = "Username" },
                        new Entry()
                            .FillExpandHorizontal()
                            .Bind(Entry.TextProperty, nameof(ViewModel.Username)),
                    }.Row(Row.Username),
                    new VerticalStackLayout
                    {
                        new Label { Text = "Password" },
                        new Entry()
                            .FillExpandHorizontal()
                            .Bind(Entry.TextProperty, nameof(ViewModel.Password)),
                    }.Row(Row.Password),
                    new VerticalStackLayout
                    {
                        new Button { Text = "Login" }
                            .CenterHorizontal()
                            .Bind(Button.CommandProperty, nameof(ViewModel.LoginCommand)),
                    }.Row(Row.LoginButton),
                    new VerticalStackLayout
                    {
                        new Label()
                            .CenterHorizontal()
                            .Bind(Label.TextProperty, nameof(ViewModel.ErrorMessage))
                    }.Row(Row.ErrorMessage)
                }
            }
        };
    }

    private enum Row
    {
        Server,
        Username,
        Password,
        LoginButton,
        ErrorMessage
    }
}
