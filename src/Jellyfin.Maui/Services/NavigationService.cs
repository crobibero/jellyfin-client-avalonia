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
    // Window is initialized on startup.
    private Window _window = null!;
    private NavigationPage? _navigationPage;

    /// <inheritdoc />
    public void Initialize(Window window)
    {
        // TODO switch to proper dispatcher in preview-11
        _window = window;
    }

    /// <inheritdoc />
    public void Navigate<TPage, TViewModel>(Guid id)
        where TViewModel : BaseIdViewModel
        where TPage : BaseContentIdPage<TViewModel>
    {
        if (_navigationPage is null)
        {
            NavigateHome();
            return;
        }

        Device.BeginInvokeOnMainThread(() =>
        {
            var resolvedView = ServiceProvider.GetService<TPage>();
            resolvedView.Initialize(id);
            _navigationPage.PushAsync(resolvedView, true).SafeFireAndForget();
        });
    }

    /// <inheridoc />
    public void NavigateToLoginPage()
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            var loginPage = ServiceProvider.GetService<LoginPage>();
            _navigationPage = null;
            _window.Page = loginPage;
        });
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
        if (_navigationPage is null)
        {
            NavigateHome();
            return;
        }

        Device.BeginInvokeOnMainThread(() =>
        {
            var resolvedView = ServiceProvider.GetService<T>();
            _navigationPage.PushAsync(resolvedView, true).SafeFireAndForget();
        });
    }

    /// <inheritdoc />
    public void NavigateHome()
    {
        if (_navigationPage is null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var homePage = ServiceProvider.GetService<HomePage>();
                homePage.Initialize();
                _window.Page = _navigationPage = new NavigationPage(homePage);
            });
        }
        else
        {
            Device.BeginInvokeOnMainThread(() => _navigationPage.PopToRootAsync(true).SafeFireAndForget());
        }
    }
}
