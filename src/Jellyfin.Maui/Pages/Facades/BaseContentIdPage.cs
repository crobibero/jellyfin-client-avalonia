using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// Interface for initializing a view with an id.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
[QueryProperty(nameof(ItemId), "itemId")]
public abstract class BaseContentIdPage<TViewModel> : BaseContentPage<TViewModel>
    where TViewModel : BaseItemViewModel
{
    private string? _itemId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContentIdPage{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    protected BaseContentIdPage(TViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <summary>
    /// Gets or sets the selected Args.
    /// </summary>
    public string? ItemId
    {
        get => _itemId;

        set
        {
            if (value == null // PopToRootAsync()
                || value == _itemId) // back button
            {
                return;
            }

            _itemId = value;

            if (Guid.TryParse(value, out Guid itemId))
            {
                ViewModel.Initialize(itemId);
            }
        }
    }

    /// <summary>
    /// Initialize the view model with an id.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    public void Initialize(Guid itemId)
    {
        ViewModel.Initialize(itemId);
    }
}
