using System.Text.Json;
using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class StateStorageService : IStateStorageService
{
    private const string StateKey = "jellyfin.state";
    private StateContainerModel? _stateCache;

    /// <inheritdoc />
    public async ValueTask<StateContainerModel> GetStoredStateAsync()
    {
        try
        {
            if (_stateCache is not null)
            {
                return _stateCache;
            }

            var storedState = await SecureStorage.GetAsync(StateKey)
                .ConfigureAwait(false);

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
            SecureStorage.Remove(StateKey);
        }
        else
        {
            var stateString = JsonSerializer.Serialize(stateModel);
            await SecureStorage.SetAsync(StateKey, stateString)
                .ConfigureAwait(false);
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
}
