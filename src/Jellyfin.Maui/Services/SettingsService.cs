namespace Jellyfin.Maui.Services;

/// <inheritdoc/>
public class SettingsService : ISettingsService
{
    private readonly ISecureStorage _secureStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="secureStorage">Instance of the <see cref="ISecureStorage"/> interface.</param>
    public SettingsService(ISecureStorage secureStorage) => _secureStorage = secureStorage;

    /// <inheritdoc/>
    public Task<string> GetAsync(string key) => _secureStorage.GetAsync(key);

    /// <inheritdoc/>
    public void Remove(string key) => _secureStorage.Remove(key);

    /// <inheritdoc/>
    public void RemoveAll() => _secureStorage.RemoveAll();

    /// <inheritdoc/>
    public Task SetAsync(string key, string value) => _secureStorage.SetAsync(key, value);
}
