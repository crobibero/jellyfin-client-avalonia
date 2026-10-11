using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// The application shell: hosts the current page, toolbar and backdrop.
/// </summary>
public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IStateService _stateService;
    private readonly IStateStorageService _stateStorageService;
    private readonly IBackdropService _backdropService;
    private readonly IImageUrlService _imageUrlService;
    private readonly IPreferencesService _preferencesService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IApplicationService _applicationService;
    private readonly PlaybackManager _playbackManager;
    private readonly Timer _clockTimer;

    [ObservableProperty]
    private BaseViewModel? _currentPage;

    [ObservableProperty]
    private bool _showToolbar;

    [ObservableProperty]
    private ToolbarItem _activeToolbarItem;

    [ObservableProperty]
    private string? _userName;

    [ObservableProperty]
    private string? _userImageUrl;

    [ObservableProperty]
    private string? _backdropUrl;

    [ObservableProperty]
    private string _clockText = string.Empty;

    [ObservableProperty]
    private bool _isClockVisible = true;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _showBackdrop;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="stateService">The state service.</param>
    /// <param name="stateStorageService">The state storage service.</param>
    /// <param name="backdropService">The backdrop service.</param>
    /// <param name="imageUrlService">The image url service.</param>
    /// <param name="preferencesService">The preferences service.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="playbackManager">The playback manager.</param>
    public MainViewModel(
        INavigationService navigationService,
        IStateService stateService,
        IStateStorageService stateStorageService,
        IBackdropService backdropService,
        IImageUrlService imageUrlService,
        IPreferencesService preferencesService,
        IAuthenticationService authenticationService,
        IApplicationService applicationService,
        PlaybackManager playbackManager)
    {
        _navigationService = navigationService;
        _stateService = stateService;
        _stateStorageService = stateStorageService;
        _backdropService = backdropService;
        _imageUrlService = imageUrlService;
        _preferencesService = preferencesService;
        _authenticationService = authenticationService;
        _applicationService = applicationService;
        _playbackManager = playbackManager;

        _navigationService.Navigated += OnNavigated;
        _stateService.UserChanged += OnUserChanged;
        _backdropService.BackdropChanged += OnBackdropChanged;
        _preferencesService.PreferencesChanged += OnPreferencesChanged;

        UpdateClock();
        _clockTimer = new Timer(_ => _applicationService.Post(UpdateClock), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    /// <summary>
    /// Start the application by restoring the previous session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task StartAsync() => _navigationService.NavigateRootAsync<StartupViewModel>();

    /// <summary>
    /// Handle a platform back request.
    /// </summary>
    /// <returns><c>true</c> when the back request was handled.</returns>
    public async Task<bool> HandleBackAsync()
    {
        if (CurrentPage is IBackHandler handler && await handler.HandleBackAsync().ConfigureAwait(true))
        {
            return true;
        }

        return await _navigationService.GoBackAsync().ConfigureAwait(true);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _clockTimer.Dispose();
        _navigationService.Navigated -= OnNavigated;
        _stateService.UserChanged -= OnUserChanged;
        _backdropService.BackdropChanged -= OnBackdropChanged;
        _preferencesService.PreferencesChanged -= OnPreferencesChanged;
    }

    [RelayCommand]
    private async Task GoHomeAsync()
    {
        if (CurrentPage is not HomeViewModel)
        {
            await _navigationService.NavigateRootAsync<HomeViewModel>().ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task GoSearchAsync()
    {
        if (CurrentPage is not SearchViewModel)
        {
            await _navigationService.NavigateAsync<SearchViewModel>().ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task GoSettingsAsync()
    {
        if (CurrentPage is not SettingsViewModel)
        {
            await _navigationService.NavigateAsync<SettingsViewModel>().ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task GoBackAsync() => await HandleBackAsync().ConfigureAwait(true);

    [RelayCommand]
    private async Task SwitchUserAsync()
    {
        await _playbackManager.StopAsync().ConfigureAwait(true);
        await _authenticationService.LogoutAsync().ConfigureAwait(true);
        await _navigationService.NavigateRootAsync<SelectUserViewModel>().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        await _playbackManager.StopAsync().ConfigureAwait(true);
        var user = _stateService.CurrentUser;
        var server = _stateService.GetServerState();
        if (user?.Id is not null && server is not null)
        {
            await _stateStorageService.SignOutUserAsync(user.Id.Value, server.Id).ConfigureAwait(true);
        }

        await _authenticationService.LogoutAsync(revokeToken: true).ConfigureAwait(true);
        await _navigationService.NavigateRootAsync<SelectUserViewModel>().ConfigureAwait(true);
    }

    private void OnNavigated(object? sender, EventArgs e)
    {
        CurrentPage = _navigationService.Current;
        ShowToolbar = CurrentPage?.ShowToolbar ?? false;
        ActiveToolbarItem = CurrentPage?.ActiveToolbarItem ?? ToolbarItem.None;
        CanGoBack = _navigationService.CanGoBack;
        ShowBackdrop = CurrentPage?.UsesBackdrop == true;

        if (CurrentPage?.UsesBackdrop != true)
        {
            _backdropService.SetBackdrop(null);
        }
    }

    private void OnUserChanged(object? sender, EventArgs e)
    {
        _applicationService.Post(() =>
        {
            var user = _stateService.CurrentUser;
            UserName = user?.Name;
            UserImageUrl = user?.Id is null ? null : _imageUrlService.GetUserImageUrl(user.Id.Value, user.PrimaryImageTag);
        });
    }

    private void OnBackdropChanged(object? sender, EventArgs e)
        => _applicationService.Post(() => BackdropUrl = _backdropService.BackdropUrl);

    private void OnPreferencesChanged(object? sender, EventArgs e)
        => _applicationService.Post(UpdateClock);

    private void UpdateClock()
    {
        IsClockVisible = _preferencesService.UserInterface.ClockEnabled;
        ClockText = DateTime.Now.ToString("t", CultureInfo.CurrentCulture);
    }
}
