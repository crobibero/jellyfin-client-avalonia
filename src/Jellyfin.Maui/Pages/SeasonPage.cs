using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Season page.
/// </summary>
public class SeasonPage : BaseContentIdPage<SeasonViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonPage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SeasonViewModel"/>.</param>
    public SeasonPage(SeasonViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheridoc />
    protected override void InitializeLayout()
    {
    }
}
