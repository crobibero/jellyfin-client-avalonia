using Jellyfin.Maui.ViewModels;
using CommunityToolkit.Maui.Markup;

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
        Content = new StackLayout
        {
            Padding = 16,
            Children =
            {
                new Label{ Text = "Server URL"},
                new Entry{HorizontalOptions = LayoutOptions.FillAndExpand}
                    .Bind(Entry.TextProperty, nameof(ViewModel.ServerUrl)),
                new Label {Text = "Username"},

                new Entry{HorizontalOptions = LayoutOptions.FillAndExpand}
                    .Bind(Entry.TextProperty, nameof(ViewModel.Username)),
                new Label {Text = "Password"},

                new Entry{HorizontalOptions = LayoutOptions.FillAndExpand}
                    .Bind(Entry.TextProperty, nameof(ViewModel.Password)),

                new Button{Text = "Login", HorizontalOptions = LayoutOptions.Center}
                    .Bind(Button.CommandProperty, nameof(ViewModel.LoginCommand)),

                new Label{HorizontalOptions = LayoutOptions.Center}
                    .Bind(Label.TextProperty, nameof(ViewModel.ErrorMessage))
            }
        };
    }
}
