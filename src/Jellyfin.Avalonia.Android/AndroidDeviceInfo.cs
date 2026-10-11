using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Android;

/// <summary>
/// Device information for Android phones, tablets and TVs.
/// </summary>
internal sealed class AndroidDeviceInfo : IDeviceInfo
{
    /// <inheritdoc />
    public string ClientName => FormFactor == DeviceFormFactor.Tv
        ? AppConstants.Name + " (Android TV)"
        : AppConstants.Name + " (Android)";

    /// <inheritdoc />
    public string DeviceName => string.IsNullOrEmpty(Build.Model) ? "Android" : Build.Model;

    /// <inheritdoc />
    public string ClientVersion => AppConstants.Version;

    /// <inheritdoc />
    public DeviceFormFactor FormFactor { get; } = IsTelevision() ? DeviceFormFactor.Tv : DeviceFormFactor.Mobile;

    /// <inheritdoc />
    public bool SupportsServerDiscovery => true;

    private static bool IsTelevision()
    {
        var uiModeManager = Application.Context.GetSystemService(Context.UiModeService) as UiModeManager;
        return uiModeManager?.CurrentModeType == UiMode.TypeTelevision
            || Application.Context.PackageManager?.HasSystemFeature("android.software.leanback") == true;
    }
}
