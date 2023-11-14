using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="ISettingsService"/>.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _basePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jellyfin.avalonia", "settings");

    /// <inheritdoc />
    public async Task<string?> GetAsync(string key)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath))
        {
            return null;
        }

        return await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Remove(string key)
        => File.Delete(GetFilePath(key));

    /// <inheritdoc />
    public void RemoveAll()
        => Directory.Delete(_basePath);

    /// <inheritdoc />
    public async Task SetAsync(string key, string value)
    {
        Directory.CreateDirectory(_basePath);

        var filePath = GetFilePath(key);
        await File.WriteAllTextAsync(filePath, value).ConfigureAwait(false);
    }

    private string GetFilePath(string input)
        => Path.Join(_basePath, input + ".config");
}
