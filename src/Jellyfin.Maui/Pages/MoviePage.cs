using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Movie page.
/// </summary>
public class MoviePage : BaseContentIdPage<MovieViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoviePage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="MovieViewModel"/>.</param>
    public MoviePage(MovieViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheridoc />
    protected override void InitializeLayout()
    {
    }
}
