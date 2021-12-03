using AsyncAwaitBestPractices;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.ViewModels.Facades;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class NavigationService : INavigationService
{
    // Navigation page is initialized on startup.
    private NavigationPage _navigationPage = null!;

    /// <inheritdoc />
    public void Initialize(NavigationPage navigationPage)
    {
        // TODO switch to proper dispatcher in preview-11
        _navigationPage = navigationPage;
    }

    /// <inheritdoc />
    public void Navigate<TPage, TViewModel>(Guid id)
        where TViewModel : BaseIdViewModel
        where TPage : BaseContentIdPage<TViewModel>
    {
        var resolvedView = ServiceProvider.GetService<TPage>();
        resolvedView.Initialize(id);
        Device.BeginInvokeOnMainThread(() => _navigationPage.PushAsync(resolvedView, true).SafeFireAndForget());
    }

    /// <inheridoc />
    public void NavigateToItemPage(BaseItemKind itemKind, Guid itemId)
    {
        switch (itemKind)
        {
            case BaseItemKind.Movie:
                Navigate<MoviePage, MovieViewModel>(itemId);
                break;
            case BaseItemKind.Episode:
                Navigate<EpisodePage, EpisodeViewModel>(itemId);
                break;
            case BaseItemKind.Season:
                Navigate<SeasonPage, SeasonViewModel>(itemId);
                break;
            case BaseItemKind.Series:
                Navigate<SeriesPage, SeriesViewModel>(itemId);
                break;
            case BaseItemKind.CollectionFolder:
                Navigate<LibraryPage, LibraryViewModel>(itemId);
                break;
            default:
                Navigate<ItemPage, ItemViewModel>(itemId);
                break;
        }
    }

    /// <inheritdoc />
    public void Navigate<T>()
        where T : Page
    {
        var resolvedView = ServiceProvider.GetService<T>();
        Device.BeginInvokeOnMainThread(() => _navigationPage.PushAsync(resolvedView, true).SafeFireAndForget());
    }

    /// <inheritdoc />
    public void NavigateToMain()
    {
        Device.BeginInvokeOnMainThread(() => _navigationPage.PopToRootAsync(true).SafeFireAndForget());
    }
}
