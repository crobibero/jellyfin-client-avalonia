using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public class HomeViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/>.
    /// </summary>
    public HomeViewModel(ILibraryService libraryService)
    {
        _libraryService = libraryService;

        BindingBase.EnableCollectionSynchronization(ContinueWatchingCollection, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(LibrariesCollection, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(RecentlyAddedCollection, null, ObservableCollectionCallback);
    }

    /// <summary>
    /// Gets the list of items to continue watching.
    /// </summary>
    public ObservableCollection<BaseItemDto> ContinueWatchingCollection { get; } = new();

    /// <summary>
    /// Gets the list of libraries.
    /// </summary>
    public ObservableCollection<BaseItemDto> LibrariesCollection { get; } = new();

    /// <summary>
    /// Gets the list of libraries and recently added items.
    /// </summary>
    public ObservableCollection<RecentlyAddedModel> RecentlyAddedCollection { get; } = new();

    /// <inheridoc />
    public override void Initialize()
    {
        _libraryService.GetContinueWatching()
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    ContinueWatchingCollection.Clear();
                    foreach (var item in task.Result)
                    {
                        ContinueWatchingCollection.Add(item);
                    }
                }
            }, TaskScheduler.Default)
            .SafeFireAndForget();

        _libraryService.GetLibraries()
            .ContinueWith(async task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    await InitializeLibraries(await task.ConfigureAwait(false)).ConfigureAwait(false);
                }
            }, TaskScheduler.Default)
            .SafeFireAndForget();
    }

    private async Task InitializeLibraries(IReadOnlyList<BaseItemDto> libraries)
    {
        foreach (var library in libraries)
        {
            LibrariesCollection.Add(library);
            var recentlyAdded = await _libraryService.GetRecentlyAdded(library.Id)
                .ConfigureAwait(false);
            RecentlyAddedCollection.Add(new RecentlyAddedModel(library.Id, library.Name, recentlyAdded));
        }
    }
}
