#pragma warning disable CS0618 // HasPassword is obsolete but still the only way to know whether a user needs a password

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// The select user view model.
/// </summary>
public partial class SelectUserViewModel : BaseViewModel
{
    private readonly IStateStorageService _stateStorageService;
    private readonly IStateService _stateService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IImageUrlService _imageUrlService;
    private readonly IImageBlurHashService _imageBlurHashService;
    private readonly PlaybackManager _playbackManager;

    [ObservableProperty]
    private string? _serverName;

    [ObservableProperty]
    private string? _serverUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="authenticationService">Instance of the <see cref="IAuthenticationService"/> interface.</param>
    /// <param name="imageUrlService">Instance of the <see cref="IImageUrlService"/> interface.</param>
    /// <param name="imageBlurHashService">Instance of the <see cref="IImageBlurHashService"/> interface.</param>
    /// <param name="playbackManager">The playback manager.</param>
    public SelectUserViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        IStateService stateService,
        IAuthenticationService authenticationService,
        IImageUrlService imageUrlService,
        IImageBlurHashService imageBlurHashService,
        PlaybackManager playbackManager)
        : base(navigationService, applicationService)
    {
        _stateStorageService = stateStorageService;
        _stateService = stateService;
        _authenticationService = authenticationService;
        _imageUrlService = imageUrlService;
        _imageBlurHashService = imageBlurHashService;
        _playbackManager = playbackManager;
    }

    /// <inheritdoc />
    public override bool ShowToolbar => false;

    /// <summary>
    /// Gets the list of users.
    /// </summary>
    public ObservableRangeCollection<UserCardModel> Users { get; } = new();

    /// <inheritdoc />
    protected override async ValueTask InitializeAsync(object? parameter) => await LoadUsersAsync(redirectIfEmpty: true).ConfigureAwait(true);

    /// <inheritdoc />
    protected override async ValueTask OnReturnedAsync() => await LoadUsersAsync(redirectIfEmpty: false).ConfigureAwait(true);

    private async Task LoadUsersAsync(bool redirectIfEmpty)
    {
        var serverState = _stateService.GetServerState();
        if (serverState is null)
        {
            await NavigationService.NavigateRootAsync<ServerSelectViewModel>().ConfigureAwait(true);
            return;
        }

        ServerName = serverState.Name;
        ServerUrl = serverState.Url;

        var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(true);
        var storedUsers = state.Users.Where(u => u.ServerId == serverState.Id)
            .OrderByDescending(u => u.LastUsed ?? DateTimeOffset.MinValue)
            .ToList();

        var publicUsers = await _authenticationService.GetPublicUsersAsync().ConfigureAwait(true);

        var users = new List<UserCardModel>();
        var missingBlurHashes = new List<UserImage>();
        foreach (var user in storedUsers)
        {
            var publicUser = publicUsers.FirstOrDefault(p => p.Id == user.Id);
            var tag = publicUser?.PrimaryImageTag ?? user.ImageTag;
            var imageUrl = _imageUrlService.GetUserImageUrl(user.Id, tag, serverState.Url);
            var blurHash = tag == user.ImageTag ? user.ImageBlurHash : null;
            if (imageUrl is not null && blurHash is null)
            {
                missingBlurHashes.Add(new UserImage(user, tag!, imageUrl));
            }

            users.Add(new UserCardModel(
                user.Id,
                user.Name,
                imageUrl,
                !string.IsNullOrEmpty(user.Token),
                publicUser?.HasPassword ?? true)
            {
                ImageBlurHash = blurHash,
            });
        }

        foreach (var user in publicUsers)
        {
            if (user.Id is null || users.Any(u => u.Id == user.Id))
            {
                continue;
            }

            users.Add(new UserCardModel(
                user.Id.Value,
                user.Name ?? string.Empty,
                _imageUrlService.GetUserImageUrl(user.Id.Value, user.PrimaryImageTag, serverState.Url),
                false,
                user.HasPassword ?? true));
        }

        Users.ReplaceRange(users);

        if (missingBlurHashes.Count > 0)
        {
            StoreBlurHashesAsync(missingBlurHashes).SafeFireAndForget();
        }

        // Only redirect on first entry, otherwise going back from the login page would land here and redirect again.
        if (redirectIfEmpty && Users.Count == 0)
        {
            // No known users, go straight to manual login.
            await NavigationService.NavigateAsync<LoginViewModel>().ConfigureAwait(true);
        }
    }

    // The server has no blurhashes for user images, so compute them now for the next visit.
    private async Task StoreBlurHashesAsync(List<UserImage> images)
    {
        var changed = false;
        foreach (var image in images)
        {
            if (await _imageBlurHashService.GetBlurHashAsync(image.Url).ConfigureAwait(true) is { } blurHash)
            {
                image.User.ImageTag = image.Tag;
                image.User.ImageBlurHash = blurHash;
                changed = true;
            }
        }

        if (changed)
        {
            var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(true);
            await _stateStorageService.SetStoredStateAsync(state).ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task AddUserAsync()
    {
        await NavigationService.NavigateAsync<LoginViewModel>().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task ChangeServerAsync()
    {
        await NavigationService.NavigateRootAsync<ServerSelectViewModel>().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task SelectUserAsync(UserCardModel? user)
    {
        if (user is null)
        {
            return;
        }

        var server = _stateService.GetServerState();
        if (server is null)
        {
            return;
        }

        if (user.HasStoredSession)
        {
            Loading = true;
            try
            {
                var state = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(true);
                var stored = state.Users.First(u => u.Id == user.Id && u.ServerId == server.Id);
                if (await _authenticationService.IsAuthenticatedAsync(server.Url, stored.Token!).ConfigureAwait(true))
                {
                    stored.LastUsed = DateTimeOffset.UtcNow;
                    _stateService.SetUserState(stored);
                    await _stateStorageService.AddUserAsync(stored).ConfigureAwait(true);
                    _playbackManager.ReportCapabilitiesAsync().SafeFireAndForget();
                    await NavigationService.NavigateRootAsync<HomeViewModel>().ConfigureAwait(true);
                    return;
                }
            }
            finally
            {
                Loading = false;
            }
        }

        await NavigationService.NavigateAsync<LoginViewModel>(new LoginRequest(user.Name, user.HasPassword)).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task DeleteUserAsync(UserCardModel? user)
    {
        var server = _stateService.GetServerState();
        if (user is not null && server is not null)
        {
            await _stateStorageService.RemoveUserAsync(user.Id, server.Id).ConfigureAwait(true);
            await LoadUsersAsync(redirectIfEmpty: false).ConfigureAwait(true);
        }
    }
}

/// <summary>
/// A stored user's image.
/// </summary>
/// <param name="User">The stored user.</param>
/// <param name="Tag">The image tag.</param>
/// <param name="Url">The image url.</param>
internal sealed record UserImage(UserStateModel User, string Tag, string Url);

/// <summary>
/// Parameters for the login page.
/// </summary>
/// <param name="Username">The user name to prefill.</param>
/// <param name="HasPassword">Whether the user has a password.</param>
public sealed record LoginRequest(string Username, bool HasPassword);
