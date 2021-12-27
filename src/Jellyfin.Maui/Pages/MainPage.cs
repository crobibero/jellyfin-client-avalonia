using AsyncAwaitBestPractices;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Main page.
/// </summary>
public class MainPage : ContentPage
{
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    public MainPage(
        INavigationService navigationService,
        IAuthenticationService authenticationService)
    {
        _navigationService = navigationService;
        _authenticationService = authenticationService;
    }

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        Redirect().SafeFireAndForget();
    }

    /// <inheritdoc />
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Redirect().SafeFireAndForget();
    }

    /// <summary>
    /// Redirect to proper page.
    /// </summary>
    private async ValueTask Redirect()
    {
        var isAuthenticated = await _authenticationService.IsAuthenticatedAsync()
            .ConfigureAwait(false);
        if (isAuthenticated)
        {
            _navigationService.NavigateHome();
        }
        else
        {
            _navigationService.NavigateToServerSelectPage();
        }
    }
}
