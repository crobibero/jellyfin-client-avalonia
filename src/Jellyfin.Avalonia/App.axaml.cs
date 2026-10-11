using AsyncAwaitBestPractices;
using AsyncImageLoader;
using AsyncImageLoader.Core.Caching;
using AsyncImageLoader.Core.Pipeline;
using AsyncImageLoader.Core.Transport;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Services;
using Jellyfin.Avalonia.Views;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia;

/// <summary>
/// Main application.
/// </summary>
public partial class App : Application
{
    private static IServiceProvider? _services;
    private MainView? _mainView;

    /// <summary>
    /// Gets or sets the callback platforms use to register their services.
    /// </summary>
    public static Action<IServiceCollection>? ConfigurePlatformServices { get; set; }

    /// <summary>
    /// Gets the application services.
    /// </summary>
    public static IServiceProvider Services => _services ?? throw new InvalidOperationException("The application is not initialized");

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJellyfinApp();
        ConfigurePlatformServices?.Invoke(services);
        _services = services.BuildServiceProvider();

        ConfigureImageLoader();

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow { Content = CreateMainView() };
                desktop.ShutdownRequested += (_, _) => (_services as IDisposable)?.Dispose();
                break;
            case IActivityApplicationLifetime activity:
                activity.MainViewFactory = CreateMainView;
                break;
            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = CreateMainView();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string UserImageCacheDirectory => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
        AppConstants.Identifier,
        "cache",
        "users");

    private static void ConfigureImageLoader()
    {
        var httpClient = Services.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientNames.Default);
        ImageLoader.AsyncImageLoader = ImageLoaderPipelineBuilder
            .RamCached(new MemoryImageCacheOptions
            {
                MaxItems = 300,
                SlidingExpiration = TimeSpan.FromMinutes(5),
            })
            .UseTransport(new UserImageCacheTransport(new HttpImageTransport(httpClient), UserImageCacheDirectory))
            .UseDecoder(new PooledBitmapDecoder())
            .Build();
    }

    private MainView CreateMainView()
    {
        if (_mainView is not null)
        {
            // Activity recreation (rotation, configuration changes) reuses the existing view.
            if (_mainView.Parent is ContentControl oldHost)
            {
                oldHost.Content = null;
            }

            return _mainView;
        }

        var viewModel = Services.GetRequiredService<MainViewModel>();
        _mainView = new MainView { DataContext = viewModel };
        viewModel.StartAsync().SafeFireAndForget();
        return _mainView;
    }
}
