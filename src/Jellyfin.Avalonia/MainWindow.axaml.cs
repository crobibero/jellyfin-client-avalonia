using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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
        DataContext = App.Current.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        AvaloniaXamlLoader.Load(this);
    }
}
