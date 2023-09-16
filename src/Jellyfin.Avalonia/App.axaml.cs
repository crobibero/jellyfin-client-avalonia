using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia;

/// <summary>
/// Main application.
/// </summary>
public class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="serviceProvider">Instance of the <see cref="IServiceProvider"/> interface.</param>
    public App(IServiceProvider serviceProvider)
    {
        Current = this;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the current application.
    /// </summary>
    public static new App Current { get; private set; } = null!;

    /// <summary>
    /// Gets the current service provided.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Initialize the application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                break;
            case ISingleViewApplicationLifetime singeView:
                singeView.MainView = ServiceProvider.GetRequiredService<MainView>();
                break;
            default:
                throw new NotImplementedException();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
