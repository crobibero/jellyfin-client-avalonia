using System.Text.Json;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class StateStorageService : IStateStorageService
{
    private const string StateKey = "jellyfin.state";
    private const string DeviceIdKey = "jellyfin.device.id";
    private readonly ISecureStorage _secureStorage;
    private StateContainerModel? _stateCache;
    private string? _deviceIdCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateStorageService"/> class.
    /// </summary>
    /// <param name="secureStorage">Instance of the <see cref="ISecureStorage"/> interface.</param>
    public StateStorageService(ISecureStorage secureStorage)
    {
        _secureStorage = secureStorage;
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

            return _stateCache = JsonSerializer.Deserialize<StateContainerModel>(storedState)
                                 ?? new StateContainerModel();
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
        ArgumentNullException.ThrowIfNull(serverStateModel, nameof(serverStateModel));

        var state = await GetStoredStateAsync().ConfigureAwait(false);

        var found = false;
        for (var index = 0; index < state.Servers.Count; index++)
        {
            if (state.Servers[index].Id == serverStateModel.Id)
            {
                // Update stored server.
                state.Servers[index] = serverStateModel;
                found = true;
                break;
            }
        }

        if (!found)
        {
            state.Servers.Add(serverStateModel);
        }

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask AddUserAsync(UserStateModel userStateModel)
    {
        ArgumentNullException.ThrowIfNull(userStateModel, nameof(userStateModel));

        var state = await GetStoredStateAsync().ConfigureAwait(false);

        var found = false;
        for (var index = 0; index < state.Users.Count; index++)
        {
            var current = state.Users[index];
            if (current.Id == userStateModel.Id && current.ServerId == userStateModel.ServerId)
            {
                // Update stored user.
                state.Users[index] = userStateModel;
                found = true;
                break;
            }
        }

        if (!found)
        {
            state.Users.Add(userStateModel);
        }

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask RemoveServerAsync(Guid serverId)
    {
        var state = await GetStoredStateAsync().ConfigureAwait(false);
        for (var index = 0; index < state.Servers.Count; index++)
        {
            if (state.Servers[index].Id == serverId)
            {
                // Remove all linked users.
                for (var userIndex = state.Users.Count; userIndex >= 0; userIndex--)
                {
                    if (state.Users[userIndex].ServerId == serverId)
                    {
                        state.Users.RemoveAt(userIndex);
                    }
                }

                state.Servers.RemoveAt(index);
                break;
            }
        }

        await SetStoredStateAsync(state).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask RemoveUserAsync(Guid userId, Guid serverId)
    {
        var state = await GetStoredStateAsync().ConfigureAwait(false);
        for (var index = 0; index < state.Users.Count; index++)
        {
            var current = state.Users[index];
            if (current.Id == userId && current.ServerId == serverId)
            {
                state.Servers.RemoveAt(index);
                break;
            }
        }

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
            return await _secureStorage.GetAsync(key).ConfigureAwait(false);
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
            await _secureStorage.SetAsync(key, value).ConfigureAwait(false);
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
            _secureStorage.Remove(key);
        }
        catch (Exception)
        {
            // Catch all exceptions, SecureStorage doesn't currenlt work on MacCatalyst.
        }
    }
}
