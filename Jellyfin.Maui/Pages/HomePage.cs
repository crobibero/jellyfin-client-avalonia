using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;

namespace Jellyfin.Maui.Pages
{
    internal class HomePage : BaseContentPage<HomeViewModel>
    {
        public HomePage(HomeViewModel viewModel) 
            : base(viewModel, "Home")
        {
        }

        protected override void InitializeLayout()
        {
            Content = new StackLayout
            {
                Padding = 16,
                Children =
                {
                    new Button {Text = "Navigate", HorizontalOptions = LayoutOptions.Center}
                        .Bind(Button.CommandProperty, nameof(ViewModel.NavigateCommand))
                }
            };
        }
    }
}
