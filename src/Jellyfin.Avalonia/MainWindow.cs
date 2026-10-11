using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Jellyfin.Avalonia;

/// <summary>
/// The desktop main window.
/// </summary>
public class MainWindow : Window
{
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
    }

    /// <summary>
    /// Toggle full screen mode.
    /// </summary>
    public void ToggleFullScreen()
        => WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;

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
}
