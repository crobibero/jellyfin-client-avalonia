using System.Text.Json;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.Pages.Facades;

/// <summary>
/// Interface for initializing a view with an id.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
[QueryProperty(nameof(Args), "args")]
public abstract class BaseContentIdPage<TViewModel> : BaseContentPage<TViewModel>
    where TViewModel : BaseItemViewModel
{
    private string? _args;

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
    public string? Args
    {
        get => _args;

        set
        {
            if (value == null // PopToRootAsync()
                || value == _args) // back button
            {
                return;
            }

            _args = value;

            var unescaped = Uri.UnescapeDataString(value);
            var item = JsonSerializer.Deserialize<BaseItemDto>(unescaped);

            if (item != null)
            {
                ViewModel.Initialize(item);
            }
        }
    }

    /// <summary>
    /// Initialize the view model with an id.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Initialize(BaseItemDto item)
    {
        ViewModel.Initialize(item);
    }
}
