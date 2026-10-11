namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Named http clients.
/// </summary>
public static class HttpClientNames
{
    /// <summary>
    /// The default http client.
    /// </summary>
    public const string Default = "Default";

    /// <summary>
    /// The http client used to probe server addresses, without retries.
    /// </summary>
    public const string Probe = "Probe";
}
