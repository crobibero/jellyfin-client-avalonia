using System.Text.Json;
using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class StateStorageService : IStateStorageService
{
    private const string StateKey = "jellyfin.state";
    private const string DeviceIdKey = "jellyfin.device.id";
    private readonly ISettingsService _settingsService;
    private StateContainerModel? _stateCache;
    private string? _deviceIdCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateStorageService"/> class.
    /// </summary>
    /// <param name="settingsService">Instance of the <see cref="ISettingsService"/> interface.</param>
    public StateStorageService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <inheritdoc />
    public async ValueTask<StateContainerModel> GetStoredStateAsync()
    {
        try
        {
            if (_stateCache is not null)
            {
                return _stateCache;
            }

            var storedState = await GetAsync(StateKey).ConfigureAwait(false);

            if (string.IsNullOrEmpty(storedState))
            {
                return new StateContainerModel();
            }

            _stateCache = JsonSerializer.Deserialize<StateContainerModel>(storedState)
                                 ?? new StateContainerModel();

            // Remove invalid users and servers.
            _stateCache.Users.Remove(null!);
            _stateCache.Servers.Remove(null!);

            return _stateCache;
        }
        catch (JsonException)
        {
            return new StateContainerModel();
        }
    }

    /// <inheritdoc />
    public async ValueTask SetStoredStateAsync(StateContainerModel? stateModel)
    {
        _stateCache = stateModel;
        if (stateModel is null)
        {
            Remove(StateKey);
        }
        else
        {
            var stateString = JsonSerializer.Serialize(stateModel);
            await SetAsync(StateKey, stateString).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask AddServerAsync(ServerStateModel serverStateModel)
    {
        ArgumentNullException.ThrowIfNull(serverStateModel);

        var state = await GetStoredStateAsync().ConfigureAwait(false);

        var existing = state.Servers.FirstOrDefault(x => x.Id == serverStateModel.Id);

        if (existing is null)
        {
            state.Servers.Add(serverStateModel);
        }
        else
        {
            // Update stored server.
            existing.Name = serverStateModel.Name;
            existing.Url = serverStateModel.Url;
        }

        state.SelectedServerId = serverStateModel.Id;

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask AddUserAsync(UserStateModel userStateModel)
    {
        ArgumentNullException.ThrowIfNull(userStateModel);

        var state = await GetStoredStateAsync().ConfigureAwait(false);

        var existing = state.Users.FirstOrDefault(x => x.Id == userStateModel.Id && x.ServerId == userStateModel.ServerId);

        if (existing is null)
        {
            state.Users.Add(userStateModel);
        }
        else
        {
            // Update stored user.
            existing.Name = userStateModel.Name;
            existing.Token = userStateModel.Token;
        }

        state.SelectedUserId = userStateModel.Id;

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask RemoveServerAsync(Guid serverId)
    {
        var state = await GetStoredStateAsync().ConfigureAwait(false);

        var server = state.Servers.FirstOrDefault(x => x.Id == serverId);

        if (server is null)
        {
            return;
        }

        foreach (var user in state.Users.Where(x => x.ServerId == serverId).ToList())
        {
            state.Users.Remove(user);
        }

        state.Servers.Remove(server);

        state.SelectedServerId = null;

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask RemoveUserAsync(Guid userId, Guid serverId)
    {
        var state = await GetStoredStateAsync().ConfigureAwait(false);

        var user = state.Users.FirstOrDefault(x => x.Id == userId && x.ServerId == serverId);

        if (user is null)
        {
            return;
        }

        state.Users.Remove(user);

        state.SelectedUserId = null;

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetDeviceIdAsync()
    {
        if (!string.IsNullOrEmpty(_deviceIdCache))
        {
            return _deviceIdCache;
        }

        _deviceIdCache = await GetAsync(DeviceIdKey).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(_deviceIdCache))
        {
            return _deviceIdCache;
        }

        // DeviceId not set, create and store a new one.
        _deviceIdCache = Guid.NewGuid().ToString();
        await SetAsync(DeviceIdKey, _deviceIdCache).ConfigureAwait(false);

        return _deviceIdCache;
    }

    private async Task<string?> GetAsync(string key)
    {
        try
        {
            return await _settingsService.GetAsync(key).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Catch all exceptions, SecureStorage doesn't currenlt work on MacCatalyst.
            return null;
        }
    }

    private async Task SetAsync(string key, string value)
    {
        try
        {
            await _settingsService.SetAsync(key, value).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Catch all exceptions, SecureStorage doesn't currenlt work on MacCatalyst.
        }
    }

    private void Remove(string key)
    {
        try
        {
            _settingsService.Remove(key);
        }
        catch (Exception)
        {
            // Catch all exceptions, SecureStorage doesn't currenlt work on MacCatalyst.
        }
    }
}
