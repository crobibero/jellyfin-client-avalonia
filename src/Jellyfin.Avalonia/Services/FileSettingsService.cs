using System.Collections.Concurrent;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Stores settings as files in the application data folder.
/// </summary>
public class FileSettingsService : ISettingsService
{
    private readonly string _basePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
        AppConstants.Identifier,
        "settings");

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _writeLocks = new(StringComparer.Ordinal);

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
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, string value)
    {
        // Saves of one key can overlap, serialize them so the newest value is written last.
        var writeLock = _writeLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await writeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(_basePath);

            // Write to a temporary file first so a crash never leaves a truncated settings file.
            var filePath = GetFilePath(key);
            var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
            await File.WriteAllTextAsync(tempPath, value).ConfigureAwait(false);
            File.Move(tempPath, filePath, overwrite: true);
        }
        finally
        {
            writeLock.Release();
        }
    }

    private string GetFilePath(string key)
        => Path.Join(_basePath, key + ".config");
}
