using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Avalonia;
using AvaloniaInside.Shell;
using AvaloniaInside.Shell.Presenters;
using Jellyfin.Avalonia.Services;
using Jellyfin.Avalonia.ViewModels;
using Jellyfin.Avalonia.Views;
using Jellyfin.Avalonia.Views.Login;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Mvvm.ViewModels.Login;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Splat;

namespace Jellyfin.Avalonia;

/// <summary>
/// Main program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Initialization code. Don't use any Avalonia, third-party APIs or any
    /// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    /// yet and stuff might break.
    /// </summary>
    /// <param name="args">The program arguments.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Logger.ForContext<App>().Fatal(ex, "Something went wrong");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    /// <returns>The app builder.</returns>
    private static AppBuilder BuildAvaloniaApp()
    {
        var serviceProvider = BuildServiceProvider();
        return AppBuilder
            .Configure(() => new App(serviceProvider))
            .UsePlatformDetect();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        BuildLogger();
        services.AddNavigation();
        services.AddSdkClients();
        services.AddServices();
        services.AddViewsAndModels();
        services.AddLogging(c => c.AddSerilog());

        return services.BuildServiceProvider();
    }

    private static void AddNavigation(this IServiceCollection services)
    {
        services.AddSingleton<INavigationRegistrar, NavigationRegistrar>();
        Locator.CurrentMutable.Register(() => App.Current.ServiceProvider.GetRequiredService<INavigationRegistrar>());

        services.AddSingleton<IPresenterProvider, PresenterProvider>();
        Locator.CurrentMutable.Register(() => App.Current.ServiceProvider.GetRequiredService<IPresenterProvider>());

        services.AddSingleton<INavigationViewLocator, DefaultNavigationViewLocator>();
        Locator.CurrentMutable.Register(() => App.Current.ServiceProvider.GetRequiredService<INavigationViewLocator>());

        services.AddSingleton<INavigationUpdateStrategy>(provider => new DefaultNavigationUpdateStrategy(provider.GetRequiredService<IPresenterProvider>()));
        Locator.CurrentMutable.Register(() => App.Current.ServiceProvider.GetRequiredService<INavigationUpdateStrategy>());

        services.AddSingleton<INavigator>(provider =>
        {
            var navigationRegistrar = provider.GetRequiredService<INavigationRegistrar>();
            return new Navigator(
                navigationRegistrar,
                new RelativeNavigateStrategy(navigationRegistrar),
                provider.GetRequiredService<INavigationUpdateStrategy>(),
                provider.GetRequiredService<INavigationViewLocator>());
        });
        Locator.CurrentMutable.Register(() => App.Current.ServiceProvider.GetRequiredService<INavigator>());
    }

    private static void AddSdkClients(this IServiceCollection services)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.1";
        // Register sdk services
        services.AddHttpClient("Default", c =>
            {
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Jellyfin-Avalonia", version));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json, 1.0));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
            })
            .ConfigurePrimaryHttpMessageHandler(_ => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8
            })
            .AddPolicyHandler(retryPolicy);

        services.AddSingleton<JellyfinSdkSettings>();
        services.AddSingleton<IAuthenticationProvider, JellyfinAuthenticationProvider>();
        services.AddScoped<IRequestAdapter, JellyfinRequestAdapter>(s => new JellyfinRequestAdapter(
            s.GetRequiredService<IAuthenticationProvider>(),
            s.GetRequiredService<JellyfinSdkSettings>(),
            s.GetRequiredService<IHttpClientFactory>().CreateClient("Default")));
        services.AddScoped<JellyfinApiClient>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        /* Avalonia Services */
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IApplicationService, ApplicationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IApplicationService, ApplicationService>();
        services.AddSingleton<ISdkService, SdkService>();

        /* Common services */
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IStateService, StateService>();
        services.AddSingleton<IStateStorageService, StateStorageService>();
    }

    private static void AddViewsAndModels(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();

        services.AddTransient<MainView>();
        services.AddTransient<MainViewModel>();

        services.AddTransient<LoadingView>();
        services.AddTransient<LoadingViewModel>();

        /* Login Views */
        services.AddTransient<ServerSelectView>();
        services.AddTransient<ServerSelectViewModel>();

        services.AddTransient<AddServerView>();
        services.AddTransient<AddServerViewModel>();

        services.AddTransient<LoginView>();
        services.AddTransient<LoginViewModel>();

        services.AddTransient<SelectUserView>();
        services.AddTransient<SelectUserViewModel>();

        /* Content Pages */
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();

        services.AddTransient<ItemView>();
        services.AddTransient<ItemViewModel>();

        services.AddTransient<LibraryViewModel>();
    }

    private static void BuildLogger()
    {
        var outputPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jellyfin.avalonia", "logs");
        const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {SourceContext}: {Message}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug(
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                path: Path.Combine(outputPath, "Jellyfin.Avalonia..log"),
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                encoding: Encoding.UTF8,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10)
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
