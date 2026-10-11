using System.Runtime.Versioning;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// Stores settings in the browser's local storage.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class BrowserSettingsService : ISettingsService
{
    private const string Prefix = "jellyfin-avalonia.";

    /// <inheritdoc />
    public Task<string?> GetAsync(string key) => Task.FromResult(BrowserInterop.GetItem(Prefix + key));

    /// <inheritdoc />
    public void Remove(string key) => BrowserInterop.RemoveItem(Prefix + key);

    /// <inheritdoc />
    public void RemoveAll() => BrowserInterop.Clear();

    /// <inheritdoc />
    public Task SetAsync(string key, string value)
    {
        BrowserInterop.SetItem(Prefix + key, value);
        return Task.CompletedTask;
    }
}
