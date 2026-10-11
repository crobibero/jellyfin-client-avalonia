using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Jellyfin.Mvvm.Preferences;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class PreferencesService : IPreferencesService
{
    private const string PlaybackKey = "jellyfin.preferences.playback";
    private const string UserInterfaceKey = "jellyfin.preferences.ui";

    private readonly ISettingsService _settingsService;
    private readonly ILogger<PreferencesService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreferencesService"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service.</param>
    /// <param name="defaults">The platform default preferences.</param>
    /// <param name="logger">The logger.</param>
    public PreferencesService(ISettingsService settingsService, IPlatformDefaults defaults, ILogger<PreferencesService> logger)
    {
        ArgumentNullException.ThrowIfNull(defaults);

        _settingsService = settingsService;
        _logger = logger;
        Playback = defaults.Playback;
        UserInterface = defaults.UserInterface;
    }

    /// <inheritdoc />
    public event EventHandler? PreferencesChanged;

    /// <inheritdoc />
    public PlaybackPreferences Playback { get; private set; }

    /// <inheritdoc />
    public UserInterfacePreferences UserInterface { get; private set; }

    /// <inheritdoc />
    public async ValueTask LoadAsync()
    {
        Playback = await ReadAsync(PlaybackKey, JsonOptions.Context.PlaybackPreferences).ConfigureAwait(false) ?? Playback;
        UserInterface = await ReadAsync(UserInterfaceKey, JsonOptions.Context.UserInterfacePreferences).ConfigureAwait(false) ?? UserInterface;
    }

    /// <inheritdoc />
    public async ValueTask SetPlaybackAsync(PlaybackPreferences preferences)
    {
        Playback = preferences;
        await WriteAsync(PlaybackKey, preferences, JsonOptions.Context.PlaybackPreferences).ConfigureAwait(false);
        PreferencesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async ValueTask SetUserInterfaceAsync(UserInterfacePreferences preferences)
    {
        UserInterface = preferences;
        await WriteAsync(UserInterfaceKey, preferences, JsonOptions.Context.UserInterfacePreferences).ConfigureAwait(false);
        PreferencesChanged?.Invoke(this, EventArgs.Empty);
    }

    private async ValueTask<T?> ReadAsync<T>(string key, JsonTypeInfo<T> typeInfo)
        where T : class
    {
        try
        {
            var json = await _settingsService.GetAsync(key).ConfigureAwait(false);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize(json, typeInfo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read preferences {Key}", key);
            return null;
        }
    }

    private async ValueTask WriteAsync<T>(string key, T value, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            await _settingsService.SetAsync(key, JsonSerializer.Serialize(value, typeInfo)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to save preferences {Key}", key);
        }
    }
}
