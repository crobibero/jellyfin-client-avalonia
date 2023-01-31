namespace Jellyfin.Maui.Services;

/// <summary>
/// ApplicationService service interface.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current value.
    /// </summary>
    /// <returns>Value.</returns>
    /// <param name="key">Key for settings.</param>
    Task<string> GetAsync(string key);

    /// <summary>
    /// Removes a desired key from the settings.
    /// </summary>
    /// <param name="key">Key for settings.</param>
    void Remove(string key);

    /// <summary>
    /// Clear all keys from settings.
    /// </summary>
    void RemoveAll();

    /// <summary>
    /// Adds or updates the value.
    /// </summary>
    /// <param name="key">Key for settings.</param>
    /// <param name="value">Value to set.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetAsync(string key, string value);
}
