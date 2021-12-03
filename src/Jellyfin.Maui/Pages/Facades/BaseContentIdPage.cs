using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// Interface for initializing a view with an id.
/// </summary>
public abstract class BaseContentIdPage<TViewModel> : BaseContentPage<TViewModel>
    where TViewModel : BaseIdViewModel
{
    /// <inheridoc />
    protected BaseContentIdPage(TViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheridoc />
    protected BaseContentIdPage(TViewModel viewModel, string pageTitle)
        : base(viewModel, pageTitle)
    {
    }

    /// <summary>
    /// Initialize the view model with an id.
    /// </summary>
    /// <param name="id">The id.</param>
    public void Initialize(Guid id)
    {
        ViewModel.Initialize(id);
    }
}
