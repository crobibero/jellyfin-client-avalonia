using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public class HomeViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;

    private IReadOnlyList<BaseItemDto>? _continueWatching;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/>.
    /// </summary>
    public HomeViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Gets or sets the list of items to continue watching.
    /// </summary>
    public IReadOnlyList<BaseItemDto>? ContinueWatching
    {
        get => _continueWatching;
        set => SetProperty(ref _continueWatching, value);
    }

    /// <inheridoc />
    public override async ValueTask InitializeAsync()
    {
        await _libraryService.GetContinueWatching()
            .ContinueWith(completed =>
            {
                if (completed.IsCompletedSuccessfully)
                {
                    ContinueWatching = completed.Result;
                }
            }, TaskScheduler.Default)
            .ConfigureAwait(false);
    }
}
