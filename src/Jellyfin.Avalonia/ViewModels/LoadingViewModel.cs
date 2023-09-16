using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Avalonia.ViewModels;

/// <summary>
/// Loading view model.
/// </summary>
public class LoadingViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ISdkService _sdkService;
    private readonly IStateService _stateService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="sdkService">Instance of the <see cref="ISdkService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    public LoadingViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IAuthenticationService authenticationService,
        ISdkService sdkService,
        IStateService stateService)
        : base(navigationService, applicationService)
    {
        _navigationService = navigationService;
        _authenticationService = authenticationService;
        _sdkService = sdkService;
        _stateService = stateService;
    }

    /// <inheritdoc />
    public override async ValueTask InitializeAsync()
    {
        await _sdkService.InitializeAsync().ConfigureAwait(true);
        await _stateService.InitializeAsync().ConfigureAwait(true);

        if (await _authenticationService.IsAuthenticatedAsync().ConfigureAwait(false))
        {
            _navigationService.NavigateHome();
        }
        else
        {
            _navigationService.NavigateToServerSelectPage();
        }
    }
}
