using System.Runtime.InteropServices;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Default device information for desktop platforms.
/// </summary>
public class DefaultDeviceInfo : IDeviceInfo
{
    /// <inheritdoc />
    public virtual string ClientName => $"{AppConstants.Name} ({GetPlatformName()})";

    /// <inheritdoc />
    public virtual string DeviceName
    {
        get
        {
            try
            {
                return Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                return GetPlatformName();
            }
        }
    }

    /// <inheritdoc />
    public virtual string ClientVersion => AppConstants.Version;

    /// <inheritdoc />
    public virtual DeviceFormFactor FormFactor => DeviceFormFactor.Desktop;

    /// <inheritdoc />
    public virtual bool SupportsServerDiscovery => true;

    private static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "Windows";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macOS";
        }

        if (OperatingSystem.IsLinux())
        {
            return "Linux";
        }

        return RuntimeInformation.OSDescription;
    }
}
