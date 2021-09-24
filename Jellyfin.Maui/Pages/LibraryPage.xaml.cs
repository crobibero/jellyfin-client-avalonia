#nullable disable

using System;
using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Pages
{
    /// <summary>
    /// The login page.
    /// </summary>
    public partial class LibraryPage : ContentPage, IInitializeId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryPage"/> class.
        /// </summary>
        /// <param name="viewModel">Instance of the <see cref="LibraryViewModel"/>.</param>
        public LibraryPage(LibraryViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public LibraryViewModel ViewModel { get; set; }

        public void Initialize(Guid id)
        {
            ViewModel.Initialize(id);
        }
    }
}