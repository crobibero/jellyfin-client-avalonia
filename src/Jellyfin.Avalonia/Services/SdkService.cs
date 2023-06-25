using System.Runtime.InteropServices;
using Jellyfin.Mvvm.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="ISdkService"/>.
/// </summary>
public class SdkService : ISdkService
{
    private readonly SdkClientSettings _sdkClientSettings;
    private readonly IStateStorageService _stateStorageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SdkService"/> class.
    /// </summary>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    public SdkService(SdkClientSettings sdkClientSettings, IStateStorageService stateStorageService)
    {
        _sdkClientSettings = sdkClientSettings;
        _stateStorageService = stateStorageService;
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.1";
        var deviceId = await _stateStorageService.GetDeviceIdAsync().ConfigureAwait(false);
        var clientName = $"Jellyfin Avalonia ({RuntimeInformation.OSDescription})";
        var deviceName = Environment.MachineName;

        _sdkClientSettings.InitializeClientSettings(
            clientName,
            version,
            deviceName,
            deviceId);
    }
}
