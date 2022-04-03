using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.ViewModels.Facades;

/// <summary>
/// ViewModel that has an ID parameter.
/// </summary>
public abstract partial class BaseIdViewModel : BaseViewModel
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private BaseItemDto _item = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseIdViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    protected BaseIdViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }

    /// <summary>
    /// Initialize the view model's id.
    /// </summary>
    /// <param name="id">The id.</param>
    public void Initialize(Guid id)
    {
        Id = id;
        InitializeAsync().SafeFireAndForget();
    }

    /// <summary>
    /// Initialize the view model's item.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Initialize(BaseItemDto item)
    {
        Id = item.Id;
        Item = item;
        InitializeAsync().SafeFireAndForget();
    }
}
