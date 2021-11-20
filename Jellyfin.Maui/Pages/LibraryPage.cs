using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using System;

namespace Jellyfin.Maui.Pages
{
    internal class LibraryPage : BaseContentPage<LibraryViewModel>, IInitializeId
    {
        public LibraryPage(LibraryViewModel viewModel) 
            : base(viewModel, "Library")
        {
        }

        public void Initialize(Guid id)
        {
            ViewModel.Initialize(id);
        }

        protected override void InitializeLayout()
        {
            Content = new StackLayout
            {
                Padding = 16,
                Children =
                {
                    new Label()
                        .Bind(Label.TextProperty, nameof(ViewModel.Id), BindingMode.OneWay)
                }
            };
        }
    }
}
