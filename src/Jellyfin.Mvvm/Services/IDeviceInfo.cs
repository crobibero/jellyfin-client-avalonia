namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Information about the device the client runs on.
/// </summary>
public interface IDeviceInfo
{
    /// <summary>
    /// Gets the client name reported to the server.
    /// </summary>
    string ClientName { get; }

    /// <summary>
    /// Gets the device name reported to the server.
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Gets the client version.
    /// </summary>
    string ClientVersion { get; }

    /// <summary>
    /// Gets the form factor of the device.
    /// </summary>
    DeviceFormFactor FormFactor { get; }

    /// <summary>
    /// Gets a value indicating whether the device can discover servers on the local network.
    /// </summary>
    bool SupportsServerDiscovery { get; }
}

/// <summary>
/// The form factor of a device.
/// </summary>
public enum DeviceFormFactor
{
    /// <summary>
    /// A desktop or laptop computer.
    /// </summary>
    Desktop = 0,

    /// <summary>
    /// A phone or tablet.
    /// </summary>
    Mobile = 1,

    /// <summary>
    /// A television, operated with a remote.
    /// </summary>
    Tv = 2,

    /// <summary>
    /// A web browser.
    /// </summary>
    Web = 3
}
