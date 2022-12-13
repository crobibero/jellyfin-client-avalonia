using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.ViewModels.Facades;

/// <summary>
/// ViewModel that has an ID parameter.
/// </summary>
public abstract partial class BaseItemViewModel : BaseViewModel
{
    [ObservableProperty]
    private BaseItemDto _item = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseItemViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    protected BaseItemViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }

    /// <summary>
    /// Initialize the view model's item.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Initialize(BaseItemDto item)
    {
        Item = item;
    }
}
