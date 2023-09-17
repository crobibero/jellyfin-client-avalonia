using AsyncAwaitBestPractices;
using AvaloniaInside.Shell;
using Jellyfin.Mvvm.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="INavigationService"/>.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly INavigator _navigator;
    private readonly IApplicationService _applicationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="navigator">Instance of the <see cref="INavigator"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    public NavigationService(INavigator navigator, IApplicationService applicationService)
    {
        _navigator = navigator;
        _applicationService = applicationService;
    }

    /// <inheritdoc />
    public void NavigateToServerSelectPage()
        => _applicationService.DispatchAsync(() => _navigator.NavigateAsync("/login-server")).SafeFireAndForget();

    /// <inheritdoc />
    public void NavigateToAddServerPage()
        => _applicationService.DispatchAsync(() => _navigator.NavigateAsync("/login-server-add")).SafeFireAndForget();

    /// <inheritdoc />
    public void NavigateToUserSelectPage()
        => _applicationService.DispatchAsync(() => _navigator.NavigateAsync("/login-user")).SafeFireAndForget();

    /// <inheritdoc />
    public void NavigateToLoginPage()
        => _applicationService.DispatchAsync(() => _navigator.NavigateAsync("/login-user-add")).SafeFireAndForget();

    /// <inheritdoc />
    public void NavigateHome()
        => _applicationService.DispatchAsync(() => _navigator.NavigateAsync("/main")).SafeFireAndForget();

    /// <inheritdoc />
    public void NavigateRoot()
        => _applicationService.DispatchAsync(() => _navigator.NavigateAsync("/loading")).SafeFireAndForget();

    /// <inheritdoc />
    public void NavigateToItemView(BaseItemDto item)
    {
        string route = item.Type switch
        {
            _ => "/item"
        };

        _applicationService.DispatchAsync(() => _navigator.NavigateAsync(route, item.Id)).SafeFireAndForget();
    }
}
