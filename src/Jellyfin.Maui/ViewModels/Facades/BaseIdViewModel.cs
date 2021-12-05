using AsyncAwaitBestPractices;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels.Facades;

/// <summary>
/// ViewModel that has an ID parameter.
/// </summary>
public abstract class BaseIdViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;

    private Guid _id;
    private BaseItemDto? _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseIdViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    protected BaseIdViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Gets the item.
    /// </summary>
    public BaseItemDto? Item
    {
        get => _item;
        private set => SetProperty(ref _item, value);
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
    public void Initialize(Guid id) => Id = id;

    /// <inheritdoc />
    public override void Initialize()
    {
        InitializeItemAsync().SafeFireAndForget();
    }

    /// <summary>
    /// Initialize the item.
    /// </summary>
    private async ValueTask InitializeItemAsync()
    {
        Item = await _libraryService.GetItemAsync(Id, ViewModelCancellationToken)
            .ConfigureAwait(false);
    }
}
