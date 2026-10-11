using Jellyfin.Mvvm.Services;
using UIKit;

namespace Jellyfin.Avalonia.iOS;

/// <summary>
/// Device information for iPhone, iPad and Apple TV.
/// </summary>
internal sealed class AppleDeviceInfo : IDeviceInfo
{
    /// <inheritdoc />
    public string ClientName => FormFactor == DeviceFormFactor.Tv
        ? AppConstants.Name + " (tvOS)"
        : AppConstants.Name + " (iOS)";

    /// <inheritdoc />
    public string DeviceName => UIDevice.CurrentDevice.Name ?? UIDevice.CurrentDevice.Model ?? "Apple device";

    /// <inheritdoc />
    public string ClientVersion => AppConstants.Version;

    /// <inheritdoc />
    public DeviceFormFactor FormFactor => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.TV
        ? DeviceFormFactor.Tv
        : DeviceFormFactor.Mobile;

    /// <inheritdoc />
    public bool SupportsServerDiscovery => true;
}
