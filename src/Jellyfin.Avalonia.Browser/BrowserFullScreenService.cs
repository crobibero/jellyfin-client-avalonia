using System.Runtime.Versioning;
using Avalonia.Controls;
using Jellyfin.Avalonia.Services;

namespace Jellyfin.Avalonia.Browser;

/// <summary>
/// Shows the page full screen.
/// </summary>
[SupportedOSPlatform("browser")]
internal sealed class BrowserFullScreenService : IFullScreenService
{
    /// <inheritdoc />
    public bool IsSupported(TopLevel topLevel) => BrowserInterop.SupportsFullscreen();

    /// <inheritdoc />
    public bool IsFullScreen(TopLevel topLevel) => BrowserInterop.IsFullscreen();

    /// <inheritdoc />
    public void SetFullScreen(TopLevel topLevel, bool fullScreen) => BrowserInterop.SetFullscreen(fullScreen);
}
