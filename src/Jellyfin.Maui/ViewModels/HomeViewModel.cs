using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Home view model.
/// </summary>
public class HomeViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public HomeViewModel(ILibraryService libraryService, INavigationService navigationService)
    {
        _libraryService = libraryService;
        _navigationService = navigationService;

        BindingBase.EnableCollectionSynchronization(ContinueWatchingCollection, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(LibrariesCollection, null, ObservableCollectionCallback);
        BindingBase.EnableCollectionSynchronization(RecentlyAddedCollection, null, ObservableCollectionCallback);

        NavigateCommand = new RelayCommand(DoNavigateCommand);
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

    /// <summary>
    /// Gets or sets the selected BaseItemDto.
    /// </summary>
    public BaseItemDto? SelectedItem { get; set; }

    /// <summary>
    /// Gets the navigate to library command.
    /// </summary>
    public IRelayCommand NavigateCommand { get; }

    /// <summary>
    /// Initialize the view model.
    /// </summary>
    public override void Initialize()
    {
        InitializeContinueWatchingAsync().SafeFireAndForget();
        InitializeLibrariesAsync().SafeFireAndForget();
    }

    private async ValueTask InitializeContinueWatchingAsync()
    {
        var continueWatching = await _libraryService.GetContinueWatchingAsync(ViewModelCancellationToken)
            .ConfigureAwait(false);
        ContinueWatchingCollection.Clear();
        foreach (var item in continueWatching)
        {
            ContinueWatchingCollection.Add(item);
        }
    }

    private async ValueTask InitializeLibrariesAsync()
    {
        var libraries = await _libraryService.GetLibrariesAsync(ViewModelCancellationToken)
            .ConfigureAwait(false);
        foreach (var library in libraries)
        {
            LibrariesCollection.Add(library);
            var recentlyAdded = await _libraryService.GetRecentlyAddedAsync(library.Id, ViewModelCancellationToken)
                .ConfigureAwait(false);
            RecentlyAddedCollection.Add(new RecentlyAddedModel(library.Id, library.Name, recentlyAdded));
        }
    }

    private void DoNavigateCommand()
    {
        if (SelectedItem is null)
        {
            return;
        }

        _navigationService.NavigateToItemView(SelectedItem.Type, SelectedItem.Id);
    }
}
