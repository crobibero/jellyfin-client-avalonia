using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Series page.
/// </summary>
public class SeriesPage : BaseContentIdPage<SeriesViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesPage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SeriesViewModel"/>.</param>
    public SeriesPage(SeriesViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheridoc />
    protected override void InitializeLayout()
    {
    }
}
