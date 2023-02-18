using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.Logging;

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
    /// <param name="logger">Instance of the <see cref="ILogger{LibraryPage}"/>.</param>
    public LibraryPage(LibraryViewModel viewModel, ILogger<LibraryPage> logger)
        : base(viewModel, logger)
    {
        InitializeComponent();
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
    }
}
