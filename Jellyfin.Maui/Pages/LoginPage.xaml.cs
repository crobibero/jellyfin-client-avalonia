#nullable disable

using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Pages
{
    /// <summary>
    /// The login page.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        /// <param name="viewModel">Instance of the <see cref="LoginViewModel"/>.</param>
        public LoginPage(LoginViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public LoginViewModel ViewModel { get; set; }
    }
}