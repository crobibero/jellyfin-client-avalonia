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

        var defaultRows = new List<HomeRowModel>
        {
            new HomeRowModel("Libraries"),
            new HomeRowModel("Continue Watching")
        };

        var libraries = await _libraryService.GetLibrariesAsync().ConfigureAwait(false);
        foreach (var library in libraries)
        {
            defaultRows.Add(new HomeRowModel(library.Name));
        }

        HomeRowCollection = defaultRows;

        defaultRows[0].Items = libraries;

        var continueWatching = await _libraryService.GetContinueWatchingAsync().ConfigureAwait(false);
        defaultRows[1].Items = continueWatching;

        for (var i = 0; i < libraries.Count; i++)
        {
            var library = libraries[i];
            var recentlyAdded = await _libraryService.GetRecentlyAddedAsync(library.Id).ConfigureAwait(false);
            defaultRows[i + 2].Items = recentlyAdded;
        }
    }
}
