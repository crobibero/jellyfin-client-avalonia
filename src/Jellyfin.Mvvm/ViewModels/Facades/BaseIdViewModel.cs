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

    /// <inheritdoc />
    protected override bool IsReady => !ItemId.Equals(default);

    /// <summary>
    /// Initialize the view model's item id.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <returns>The task.</returns>
    public async ValueTask InitializeAsync(Guid itemId)
    {
        ItemId = itemId;
        await InitializeAsync().ConfigureAwait(true);
    }
}
