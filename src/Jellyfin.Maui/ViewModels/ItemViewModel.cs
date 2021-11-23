using AsyncAwaitBestPractices;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Item view model.
/// </summary>
public class ItemViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    private BaseItemDto? _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemViewModel"/>.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/>.</param>
    public ItemViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    public BaseItemDto? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        _libraryService.GetItem(Id)
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Item = task.Result;
                }
            }, TaskScheduler.Default)
            .SafeFireAndForget();
    }
}
