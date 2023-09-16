using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Jellyfin.Avalonia;

/// <summary>
/// The main window.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
