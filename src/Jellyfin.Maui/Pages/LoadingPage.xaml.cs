using Jellyfin.Maui.Services;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// The loading page / pseudo splash screen. Used to initialize all necessary services asynchronously without blocking the ui thread.
/// </summary>
public partial class LoadingPage : ContentPage
{
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    private bool _busy;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingPage"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    public LoadingPage(
        INavigationService navigationService,
        IAuthenticationService authenticationService)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _authenticationService = authenticationService;
    }

    /// <inheritdoc/>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_busy)
        {
            return;
        }

        _busy = true;

        await InternalServiceProvider.GetService<ISdkService>().InitializeAsync()
            .ConfigureAwait(true);

        await InternalServiceProvider.GetService<IStateService>().InitializeAsync()
            .ConfigureAwait(true);

#if DEBUG
        await Task.Delay(1000)
            .ConfigureAwait(true); // mock slow perfs
#endif

        if (await _authenticationService.IsAuthenticatedAsync()
            .ConfigureAwait(true))
        {
            _navigationService.NavigateHome();
        }
        else
        {
            _navigationService.NavigateToAddServerPage();
        }

        _busy = false;
    }
}
