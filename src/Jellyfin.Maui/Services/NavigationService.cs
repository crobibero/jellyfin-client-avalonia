using AsyncAwaitBestPractices;
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
        Device.BeginInvokeOnMainThread(() => _navigationPage.PushAsync(resolvedView, true).SafeFireAndForget());
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
