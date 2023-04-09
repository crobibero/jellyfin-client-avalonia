using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="ISettingsService"/>.
/// </summary>
public class SettingsService : ISettingsService
{
    /// <inheritdoc />
    public Task<string> GetAsync(string key)
    {
        // TODO.
        return Task.FromResult(string.Empty);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        // TODO
    }

    /// <inheritdoc />
    public void RemoveAll()
    {
        // TODO
    }

    /// <inheritdoc />
    public Task SetAsync(string key, string value)
    {
        // TODO
        return Task.CompletedTask;
    }
}
