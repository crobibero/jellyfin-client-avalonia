using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// The add server page.
/// </summary>
public partial class LibraryPage : BaseContentIdPage<LibraryViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="LibraryViewModel"/>.</param>
    public LibraryPage(LibraryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
    }
}
