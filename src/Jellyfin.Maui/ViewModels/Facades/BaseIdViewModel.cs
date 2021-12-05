using AsyncAwaitBestPractices;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels.Facades;

/// <summary>
/// ViewModel that has an ID parameter.
/// </summary>
public abstract class BaseIdViewModel : BaseViewModel
{
    private Guid _id;
    private BaseItemDto? _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseIdViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    protected BaseIdViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    public BaseItemDto? Item
    {
        get => _item;
        protected set => SetProperty(ref _item, value);
    }

    /// <summary>
    /// Gets the id.
    /// </summary>
    public Guid Id
    {
        get => _id;
        private set => SetProperty(ref _id, value);
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
