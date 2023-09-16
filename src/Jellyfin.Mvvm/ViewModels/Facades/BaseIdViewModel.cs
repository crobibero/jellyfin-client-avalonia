using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Mvvm.ViewModels.Facades;

/// <summary>
/// ViewModel that has an ID parameter.
/// </summary>
public abstract partial class BaseItemViewModel : BaseViewModel
{
    [ObservableProperty]
    private BaseItemDto? _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseItemViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    protected BaseItemViewModel(INavigationService navigationService, IApplicationService applicationService)
        : base(navigationService, applicationService)
    {
    }

    /// <summary>
    /// Gets the current item id.
    /// </summary>
    protected Guid ItemId { get; private set; }

    /// <summary>
    /// Initialize the view model's item id.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    public void InitializeItemId(Guid itemId)
    {
        ItemId = itemId;
    }
}
