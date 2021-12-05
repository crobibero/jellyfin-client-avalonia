using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// Interface for initializing a view with an id.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
public abstract class BaseContentIdPage<TViewModel> : BaseContentPage<TViewModel>
    where TViewModel : BaseIdViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentIdPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    protected BaseContentIdPage(TViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentIdPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    /// <param name="pageTitle">The page title.</param>
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
