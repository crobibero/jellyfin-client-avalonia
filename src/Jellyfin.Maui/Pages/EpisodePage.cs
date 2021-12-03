using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Episode page.
/// </summary>
public class EpisodePage : BaseContentIdPage<EpisodeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodePage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="EpisodeViewModel"/>.</param>
    public EpisodePage(EpisodeViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheridoc />
    protected override void InitializeLayout()
    {
    }
}
