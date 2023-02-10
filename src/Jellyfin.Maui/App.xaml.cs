using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Services;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Maui;

/// <summary>
/// The main application.
/// </summary>
public partial class App : Application
{
    private readonly ILogger<App> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="Logger{App}"/> interface.</param>
    public App(ILogger<App> logger)
    {
        _logger = logger;

        InitializeComponent();

        MainPage = InternalServiceProvider.GetService<LoadingPage>();
        MauiExceptions.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                _logger.LogCritical(ex, "Fatal Error");
            }
            else
            {
                _logger.LogCritical("Fatal Error {@Message}", args.ExceptionObject);
            }
        };
    }
}
