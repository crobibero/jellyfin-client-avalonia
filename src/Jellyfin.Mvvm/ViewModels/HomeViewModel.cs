using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public partial class HomeViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;
    private readonly IStateService _stateService;
    private DateTime? _updateTimestamp;

    [ObservableProperty]
    private IReadOnlyList<HomeRowModel> _homeRowCollection = Array.Empty<HomeRowModel>();

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public HomeViewModel(
        ILibraryService libraryService,
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateService stateService)
        : base(navigationService, applicationService)
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
            return;
        }

        _updateTimestamp = now;

        var displayPreferences = await _libraryService.GetDisplayPreferencesAsync().ConfigureAwait(false);

        var userConfig = _stateService.GetCurrentUser().Configuration;

        var libraryViews = new List<Guid>();
        var excludedViews = new List<Guid>();
        var libraries = await _libraryService.GetLibrariesAsync().ConfigureAwait(false);
        foreach (var view in userConfig.LatestItemsExcludes)
        {
            excludedViews.Add(view);
        }

        foreach (var view in userConfig.OrderedViews)
        {
            if (!excludedViews.Contains(view))
            {
                libraryViews.Add(view);
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

                            var row = new HomeRowModel(string.Format(CultureInfo.InvariantCulture, Strings.Strings.Home_Latest, library.Name));
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
    }

    private static async ValueTask PopulateRowAsync(HomeRowModel homeRowModel, Func<ValueTask<IReadOnlyList<BaseItemDto>>> populateFunction)
    {
        homeRowModel.Items = await populateFunction().ConfigureAwait(false);
    }
}
