using Jellyfin.Mvvm.Services;

namespace Jellyfin.Maui.Services;

/// <summary>
/// Implementation of the <see cref="ISdkService"/> interface.
/// </summary>
public class SdkService : ISdkService
{
    private readonly IDeviceInfo _deviceInfo;
    private readonly IStateStorageService _stateStorageService;
    private readonly SdkClientSettings _sdkClientSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SdkService"/> class.
    /// </summary>
    /// <param name="deviceInfo">Instance of the <see cref="IDeviceInfo"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    public SdkService(
        IDeviceInfo deviceInfo,
        IStateStorageService stateStorageService,
        SdkClientSettings sdkClientSettings)
    {
        _deviceInfo = deviceInfo;
        _stateStorageService = stateStorageService;
        _sdkClientSettings = sdkClientSettings;
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        var version = typeof(MauiProgram).Assembly.GetName().Version?.ToString() ?? "0.0.0.1";
        var deviceId = await _stateStorageService.GetDeviceIdAsync().ConfigureAwait(false);
        var clientName = $"Jellyfin Maui ({_deviceInfo.Idiom} - {_deviceInfo.Platform})";

        _sdkClientSettings.InitializeClientSettings(
            clientName,
            version,
            _deviceInfo.Name,
            deviceId);
    }
}
