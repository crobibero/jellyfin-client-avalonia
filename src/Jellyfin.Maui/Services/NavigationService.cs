#pragma warning disable

using AsyncAwaitBestPractices;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.ViewModels.Facades;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class NavigationService  : INavigationService
{
    // Application is initialized on startup.
    private Application _application = null!;
    private NavigationPage? _navigationPage;

    /// <inheritdoc />
    public void Initialize(Application application)
    {
        _application = application;
    }

    /// <inheritdoc />
    public void NavigateToLoginPage()
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            var loginPage = ServiceProvider.GetService<LoginPage>();
            _navigationPage = null;
            _application.MainPage = loginPage;
        });
    }

    /// <inheritdoc />
    public void NavigateToItemView(BaseItemKind itemKind, Guid itemId)
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
    public void NavigateHome()
    {
        if (_navigationPage is null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var homePage = ServiceProvider.GetService<HomePage>();
                homePage.Initialize();
                _application.MainPage = _navigationPage = new NavigationPage(homePage);
            });
        }
        else
        {
            Device.BeginInvokeOnMainThread(() => _navigationPage.PopToRootAsync(true).SafeFireAndForget());
        }
    }

    private void Navigate<TPage, TViewModel>(Guid id)
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
}
