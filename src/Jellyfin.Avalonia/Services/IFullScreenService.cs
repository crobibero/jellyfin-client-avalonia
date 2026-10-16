using Avalonia.Controls;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Shows the app full screen on platforms where it runs in a window.
/// </summary>
public interface IFullScreenService
{
    /// <summary>
    /// Gets a value indicating whether the app can switch to full screen.
    /// </summary>
    /// <param name="topLevel">The top level showing the app.</param>
    /// <returns>Whether full screen is supported.</returns>
    bool IsSupported(TopLevel topLevel);

    /// <summary>
    /// Gets a value indicating whether the app is full screen.
    /// </summary>
    /// <param name="topLevel">The top level showing the app.</param>
    /// <returns>Whether the app is full screen.</returns>
    bool IsFullScreen(TopLevel topLevel);

    /// <summary>
    /// Enter or leave full screen.
    /// </summary>
    /// <param name="topLevel">The top level showing the app.</param>
    /// <param name="fullScreen">Whether to show the app full screen.</param>
    void SetFullScreen(TopLevel topLevel, bool fullScreen);
}

/// <summary>
/// Full screen for the desktop main window.
/// </summary>
public class WindowFullScreenService : IFullScreenService
{
    /// <inheritdoc />
    public bool IsSupported(TopLevel topLevel) => topLevel is MainWindow;

    /// <inheritdoc />
    public bool IsFullScreen(TopLevel topLevel) => topLevel is MainWindow { WindowState: WindowState.FullScreen };

    /// <inheritdoc />
    public void SetFullScreen(TopLevel topLevel, bool fullScreen)
    {
        if (topLevel is MainWindow window && IsFullScreen(window) != fullScreen)
        {
            window.ToggleFullScreen();
        }
    }
}
