using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// JavaScript interop for the browser host.
/// </summary>
[SupportedOSPlatform("browser")]
internal static partial class BrowserInterop
{
    /// <summary>
    /// The player module name.
    /// </summary>
    public const string PlayerModule = "jellyfin-player";

    [JSImport("globalThis.localStorage.getItem")]
    public static partial string? GetItem(string key);

    [JSImport("globalThis.localStorage.setItem")]
    public static partial void SetItem(string key, string value);

    [JSImport("globalThis.localStorage.removeItem")]
    public static partial void RemoveItem(string key);

    [JSImport("globalThis.localStorage.clear")]
    public static partial void Clear();

    [JSImport("globalThis.navigator.userAgent.toString")]
    public static partial string GetUserAgent();

    [JSImport("canPlayType", PlayerModule)]
    public static partial string CanPlayType(string type);

    [JSImport("isTypeSupported", PlayerModule)]
    public static partial bool IsTypeSupported(string type);

    [JSImport("supportsNativeHls", PlayerModule)]
    public static partial bool SupportsNativeHls();

    [JSImport("supportsAssRenderer", PlayerModule)]
    public static partial bool SupportsAssRenderer();

    [JSImport("supportsFullscreen", PlayerModule)]
    public static partial bool SupportsFullscreen();

    [JSImport("isFullscreen", PlayerModule)]
    public static partial bool IsFullscreen();

    [JSImport("setFullscreen", PlayerModule)]
    public static partial void SetFullscreen(bool fullscreen);

    [JSImport("load", PlayerModule)]
    public static partial Task Load(string url, bool isHls, double startSeconds);

    [JSImport("play", PlayerModule)]
    public static partial void Play();

    [JSImport("pause", PlayerModule)]
    public static partial void Pause();

    [JSImport("stop", PlayerModule)]
    public static partial void Stop();

    [JSImport("seek", PlayerModule)]
    public static partial void Seek(double seconds);

    [JSImport("getState", PlayerModule)]
    public static partial string GetState();

    [JSImport("setVolume", PlayerModule)]
    public static partial void SetVolume(int volume);

    [JSImport("setMuted", PlayerModule)]
    public static partial void SetMuted(bool muted);

    [JSImport("setRate", PlayerModule)]
    public static partial void SetRate(double rate);

    [JSImport("setObjectFit", PlayerModule)]
    public static partial void SetObjectFit(string fit);

    [JSImport("loadSubtitle", PlayerModule)]
    public static partial Task LoadSubtitle(string url, string format, string? language, [JSMarshalAs<JSType.Array<JSType.String>>] string[] fonts);

    [JSImport("selectTextTrack", PlayerModule)]
    public static partial void SelectTextTrack(int index);

    [JSImport("selectAudioTrack", PlayerModule)]
    public static partial void SelectAudioTrack(int index);

    [JSImport("globalThis.matchMedia")]
    private static partial JSObject MatchMedia(string query);

    /// <summary>
    /// Evaluate a media query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>Whether the query matches.</returns>
    public static bool Matches(string query)
    {
        using var result = MatchMedia(query);
        return result.GetPropertyAsBoolean("matches");
    }
}
