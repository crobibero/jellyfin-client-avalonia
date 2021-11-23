using AsyncAwaitBestPractices;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Library view model.
/// </summary>
public class LibraryViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;

    private BaseItemDto? _library;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryViewModel"/>.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/>.</param>
    public LibraryViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Gets or sets the library.
    /// </summary>
    public BaseItemDto? Library
    {
        get => _library;
        set => SetProperty(ref _library, value);
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        _libraryService.GetLibrary(Id)
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Library = task.Result;
                }
            }, TaskScheduler.Default)
            .SafeFireAndForget();
    }
}
