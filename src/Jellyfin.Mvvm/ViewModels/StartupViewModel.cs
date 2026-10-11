using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Jellyfin.Mvvm.ViewModels.Login;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Restores the previous session and routes to the first page.
/// </summary>
public class StartupViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IStateService _stateService;
    private readonly IStateStorageService _stateStorageService;
    private readonly IPreferencesService _preferencesService;
    private readonly IDeviceInfo _deviceInfo;
    private readonly JellyfinSdkSettings _sdkSettings;
    private readonly PlaybackManager _playbackManager;
    private readonly ILogger<StartupViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="stateStorageService">The state storage service.</param>
    /// <param name="preferencesService">The preferences service.</param>
    /// <param name="deviceInfo">The device info.</param>
    /// <param name="sdkSettings">The sdk settings.</param>
    /// <param name="playbackManager">The playback manager.</param>
    /// <param name="logger">The logger.</param>
    public StartupViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IAuthenticationService authenticationService,
        IStateService stateService,
        IStateStorageService stateStorageService,
        IPreferencesService preferencesService,
        IDeviceInfo deviceInfo,
        JellyfinSdkSettings sdkSettings,
        PlaybackManager playbackManager,
        ILogger<StartupViewModel> logger)
        : base(navigationService, applicationService)
    {
        _authenticationService = authenticationService;
        _stateService = stateService;
        _stateStorageService = stateStorageService;
        _preferencesService = preferencesService;
        _deviceInfo = deviceInfo;
        _sdkSettings = sdkSettings;
        _playbackManager = playbackManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public override bool ShowToolbar => false;

    /// <inheritdoc />
    public override bool KeepInBackStack => false;

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter)
    {
        var deviceId = await _stateStorageService.GetDeviceIdAsync().ConfigureAwait(true);
        _sdkSettings.Initialize(_deviceInfo.ClientName, _deviceInfo.ClientVersion, _deviceInfo.DeviceName, deviceId);

        await _preferencesService.LoadAsync().ConfigureAwait(true);
        await _stateService.InitializeAsync().ConfigureAwait(true);

        if (await _authenticationService.IsAuthenticatedAsync().ConfigureAwait(true))
        {
            _logger.LogInformation("Restored session for {User}", _stateService.CurrentUser?.Name);
            _playbackManager.ReportCapabilitiesAsync().SafeFireAndForget();
            await NavigationService.NavigateRootAsync<HomeViewModel>().ConfigureAwait(true);
        }
        else if (_stateService.GetServerState() is not null)
        {
            await NavigationService.NavigateRootAsync<SelectUserViewModel>().ConfigureAwait(true);
        }
        else
        {
            await NavigationService.NavigateRootAsync<ServerSelectViewModel>().ConfigureAwait(true);
        }
    }
}
