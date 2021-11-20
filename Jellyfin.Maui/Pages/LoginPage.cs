using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;

namespace Jellyfin.Maui.Pages
{
    internal class LoginPage : BaseContentPage<LoginViewModel>
    {
        public LoginPage(LoginViewModel viewModel)
            : base(viewModel)
        {
        }

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
}
