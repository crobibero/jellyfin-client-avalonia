using Jellyfin.Maui.Pages;
using Microsoft.Maui.Essentials;

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
    public void Navigate<T>(Guid id)
        where T : Page, IInitializeId
    {
        var resolvedView = ServiceProvider.GetService<T>();
        resolvedView.Initialize(id);

        if (MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PushAsync(resolvedView, true).ConfigureAwait(true);
            });
        }
        else
        {
            App.Current?.Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PushAsync(resolvedView, true).ConfigureAwait(true);
            });
        }
    }

    /// <inheritdoc />
    public void Navigate<T>()
        where T : Page
    {
        var resolvedView = ServiceProvider.GetService<T>();
        if (MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PushAsync(resolvedView, true).ConfigureAwait(true);
            });
        }
        else
        {
            App.Current?.Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PushAsync(resolvedView, true).ConfigureAwait(true);
            });
        }
    }

    /// <inheritdoc />
    public void NavigateToMain()
    {
        if (MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PopToRootAsync(true).ConfigureAwait(true);
            });
        }
        else
        {
            App.Current?.Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _navigationPage.PopToRootAsync(true).ConfigureAwait(true);
            });
        }
    }
}
