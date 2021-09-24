#nullable disable

using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Pages
{
    /// <summary>
    /// The login page.
    /// </summary>
    public partial class HomePage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        /// <param name="viewModel">Instance of the <see cref="HomeViewModel"/>.</param>
        public HomePage(HomeViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public HomeViewModel ViewModel { get; set; }
    }
}