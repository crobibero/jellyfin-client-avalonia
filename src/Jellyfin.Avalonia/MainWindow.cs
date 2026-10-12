using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Jellyfin.Avalonia;

/// <summary>
/// The desktop main window.
/// </summary>
public class MainWindow : Window
{
    private WindowState _windowedState = WindowState.Normal;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        Title = "Jellyfin";
        Width = 1440;
        Height = 860;
        MinWidth = 640;
        MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        this[!BackgroundProperty] = this.GetResourceObservable("BackgroundBrush").ToBinding();

        using var icon = AssetLoader.Open(new Uri("avares://Jellyfin.Avalonia/Resources/Assets/jellyfin-icon.png"));
        Icon = new WindowIcon(new Bitmap(icon));

        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Toggle full screen mode, returning to the windowed or maximized state it was entered from.
    /// </summary>
    public void ToggleFullScreen()
    {
        if (WindowState == WindowState.FullScreen)
        {
            WindowState = _windowedState;
        }
        else
        {
            _windowedState = WindowState == WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
            WindowState = WindowState.FullScreen;
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Key == Key.F11 || (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.Alt))
        {
            ToggleFullScreen();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // Escape leaves full screen before the shell treats it as back, unless it is closing a popup.
        if (e.Key == Key.Escape
            && WindowState == WindowState.FullScreen
            && !e.Source.IsInPopup())
        {
            ToggleFullScreen();
            e.Handled = true;
        }
    }
}
