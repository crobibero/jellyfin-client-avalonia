using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia;

/// <summary>
/// Main application.
/// </summary>
public class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="serviceProvider">Instance of the <see cref="IServiceProvider"/> interface.</param>
    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Initialize the application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        DataTemplates.Add(_serviceProvider.GetRequiredService<ViewLocator>());
        Resources.Add(typeof(IServiceProvider), _serviceProvider);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                break;
            case ISingleViewApplicationLifetime singeView:
                singeView.MainView = _serviceProvider.GetRequiredService<MainWindow>();
                break;
            default:
                throw new NotImplementedException();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
