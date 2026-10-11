using System.Runtime.Versioning;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// Device information for browsers.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class BrowserDeviceInfo : IDeviceInfo
{
    private readonly Lazy<string> _browserName = new(GetBrowserName);

    /// <inheritdoc />
    public string ClientName => AppConstants.Name + " (Web)";

    /// <inheritdoc />
    public string DeviceName => _browserName.Value;

    /// <inheritdoc />
    public string ClientVersion => AppConstants.Version;

    /// <inheritdoc />
    public DeviceFormFactor FormFactor => DeviceFormFactor.Web;

    /// <inheritdoc />
    public bool SupportsServerDiscovery => false;

    private static string GetBrowserName()
    {
        var userAgent = BrowserInterop.GetUserAgent();
        if (userAgent.Contains("Edg/", StringComparison.Ordinal))
        {
            return "Edge";
        }

        if (userAgent.Contains("Firefox/", StringComparison.Ordinal))
        {
            return "Firefox";
        }

        if (userAgent.Contains("Chrome/", StringComparison.Ordinal))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Safari/", StringComparison.Ordinal))
        {
            return "Safari";
        }

        return "Web Browser";
    }
}
