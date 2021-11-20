using Jellyfin.Maui.Pages;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class NavigationService : INavigationService
{
    // Navigation page is initialized on startup.
    private NavigationPage _navigationPage = null!;

    /// <inheritdoc />
    public void Initialize(NavigationPage navigationPage)
    {
        _navigationPage = navigationPage;
    }

    /// <inheritdoc />
    public void Navigate<T>(Guid id)
        where T : Page, IInitializeId
    {
        var resolvedView = ServiceProvider.GetService<T>();
        resolvedView.Initialize(id);

        Application.Current!.Dispatcher.BeginInvokeOnMainThread(async () =>
        {
            await _navigationPage.PushAsync(resolvedView, true).ConfigureAwait(true);
        });
    }

    /// <inheritdoc />
    public void Navigate<T>()
        where T : Page
    {
        var resolvedView = ServiceProvider.GetService<T>();

        Application.Current!.Dispatcher.BeginInvokeOnMainThread(async () =>
        {
            await _navigationPage.PushAsync(resolvedView, true).ConfigureAwait(true);
        });
    }

    /// <inheritdoc />
    public void NavigateToMain()
    {
        Application.Current!.Dispatcher.BeginInvokeOnMainThread(async () =>
        {
            await _navigationPage.PopToRootAsync(true).ConfigureAwait(true);
        });
    }
}
