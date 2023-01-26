using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.Services;

/// <summary>
/// The state storage service.
/// </summary>
public interface IStateStorageService
{
    /// <summary>
    /// Gets the stored device id.
    /// </summary>
    /// <returns>The device id.</returns>
    ValueTask<string> GetDeviceIdAsync();

    /// <summary>
    /// Gets the stored state.
    /// </summary>
    /// <returns>The stored state.</returns>
    ValueTask<StateContainerModel> GetStoredStateAsync();

    /// <summary>
    /// Sets the stored state.
    /// </summary>
    /// <param name="stateModel">The state model.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    ValueTask SetStoredStateAsync(StateContainerModel? stateModel);

    /// <summary>
    /// Adds the server in the store.
    /// </summary>
    /// <param name="serverStateModel">The server state model.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    ValueTask AddServerAsync(ServerStateModel serverStateModel);

    /// <summary>
    /// Adds the user to the store.
    /// </summary>
    /// <param name="userStateModel">The user state model.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    ValueTask AddUserAsync(UserStateModel userStateModel);

    /// <summary>
    /// Remove the server from the store.
    /// </summary>
    /// <param name="serverId">The server id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    ValueTask RemoveServerAsync(Guid serverId);

    /// <summary>
    /// Remove the user from the store.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="serverId">The server id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    ValueTask RemoveUserAsync(Guid userId, Guid serverId);
}
