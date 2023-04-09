using Avalonia;
using Avalonia.Controls;
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
        var serviceProvider = (IServiceProvider)Application.Current!.Resources[typeof(IServiceProvider)]!;
        DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>();
        InitializeComponent();
    }
}
