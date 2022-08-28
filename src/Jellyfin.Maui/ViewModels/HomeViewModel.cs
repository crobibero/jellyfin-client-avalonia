using System.Globalization;
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
    private readonly IStateService _stateService;
    private DateTime? _updateTimestamp;

    [ObservableProperty]
    private bool _loading = true;

    [ObservableProperty]
    private IReadOnlyList<HomeRowModel> _homeRowCollection = Array.Empty<HomeRowModel>();

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public HomeViewModel(
        ILibraryService libraryService,
        INavigationService navigationService,
        IStateService stateService)
        : base(navigationService)
    {
        _libraryService = libraryService;
        _stateService = stateService;
    }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        // TODO partial update
        var now = DateTime.UtcNow;
        if (_updateTimestamp > now.AddMinutes(-5))
        {
            // Home page was updated recently.
            Loading = false;
            return;
        }

        Loading = true;
        _updateTimestamp = now;

        var displayPreferences = await _libraryService.GetDisplayPreferencesAsync().ConfigureAwait(false);

        var userConfig = _stateService.GetCurrentUser().Configuration;

        var libraryViews = new List<Guid>();
        var excludedViews = new List<Guid>();
        var libraries = await _libraryService.GetLibrariesAsync().ConfigureAwait(false);
        foreach (var view in userConfig.LatestItemsExcludes)
        {
            if (Guid.TryParse(view, out var parsed))
            {
                excludedViews.Add(parsed);
            }
        }

        foreach (var view in userConfig.OrderedViews)
        {
            if (Guid.TryParse(view, out var parsed) && !excludedViews.Contains(parsed))
            {
                libraryViews.Add(parsed);
            }
        }

        var homeRows = new List<HomeRowModel>();
        foreach (var homeSection in displayPreferences.HomeSections)
        {
            switch (homeSection)
            {
                case DisplayPreferencesModel.HomeSection.LibraryTiles:
                    {
                        var row = new HomeRowModel("Libraries");
                        row.Items = libraries;
                        homeRows.Add(row);
                        break;
                    }

                case DisplayPreferencesModel.HomeSection.Resume:
                    {
                        var row = new HomeRowModel("Continue Watching");
                        homeRows.Add(row);
                        PopulateRowAsync(row, () => _libraryService.GetContinueWatchingAsync()).SafeFireAndForget();
                        break;
                    }

                case DisplayPreferencesModel.HomeSection.NextUp:
                    {
                        var row = new HomeRowModel("Next Up");
                        homeRows.Add(row);
                        PopulateRowAsync(row, () => _libraryService.GetNextUpAsync()).SafeFireAndForget();
                        break;
                    }

                case DisplayPreferencesModel.HomeSection.LatestMedia:
                    {
                        foreach (var view in libraryViews)
                        {
                            var library = libraries.FirstOrDefault(i => i.Id == view);
                            if (library is null)
                            {
                                continue;
                            }

                            var row = new HomeRowModel(string.Format(CultureInfo.InvariantCulture, Strings.Home_Latest, library.Name));
                            homeRows.Add(row);
                            PopulateRowAsync(row, () => _libraryService.GetRecentlyAddedAsync(view)).SafeFireAndForget();
                        }

                        break;
                    }

                case DisplayPreferencesModel.HomeSection.None:
                default:
                    break;
            }
        }

        HomeRowCollection = homeRows;
        Loading = false;
    }

    private async ValueTask PopulateRowAsync(HomeRowModel homeRowModel, Func<ValueTask<IReadOnlyList<BaseItemDto>>> populateFunction)
    {
        homeRowModel.Items = await populateFunction().ConfigureAwait(false);
    }
}
