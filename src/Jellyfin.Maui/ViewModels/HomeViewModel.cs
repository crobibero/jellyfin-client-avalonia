using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public partial class HomeViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;
    private DateTime? _updateTimestamp;

    [ObservableProperty]
    private IReadOnlyList<HomeRowModel> _homeRowCollection = Array.Empty<HomeRowModel>();

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public HomeViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;

        // TODO set order by DisplayPreferences.. eventually.
        BindingBase.EnableCollectionSynchronization(HomeRowCollection, null, ObservableCollectionCallback);
    }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        // TODO partial update
        var now = DateTime.UtcNow;
        if (_updateTimestamp > now.AddMinutes(-5))
        {
            // Home page was updated recently.
            return;
        }

        _updateTimestamp = now;
        HomeRowCollection = Array.Empty<HomeRowModel>();

        var homeRows = new List<HomeRowModel>();
        var libraries = await _libraryService.GetLibrariesAsync().ConfigureAwait(false);
        homeRows.Add(new HomeRowModel("Libraries", 0, libraries));

        var continueWatching = await _libraryService.GetContinueWatchingAsync().ConfigureAwait(false);
        homeRows.Add(new HomeRowModel("Continue Watching", 0, continueWatching));

        foreach (var library in libraries)
        {
            var recentlyAdded = await _libraryService.GetRecentlyAddedAsync(library.Id).ConfigureAwait(false);
            homeRows.Add(new HomeRowModel(library.Name, 0, recentlyAdded));
        }

        HomeRowCollection = homeRows;
    }
}
