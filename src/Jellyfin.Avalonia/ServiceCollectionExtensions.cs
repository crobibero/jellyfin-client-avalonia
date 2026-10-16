using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Jellyfin.Avalonia.Services;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Sdk;
using Jellyfin.Sdk.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Jellyfin.Avalonia;

/// <summary>
/// Service registration for the shared application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register the shared application services. Platforms can replace any registration afterwards.
    /// </summary>
    /// <param name="services">The service collection to register in.</param>
    /// <returns>The same collection, for chaining.</returns>
    public static IServiceCollection AddJellyfinApp(this IServiceCollection services)
    {
        services.AddSdkClients();

        services.TryAddSingleton<IApplicationService, ApplicationService>();
        services.TryAddSingleton<ISettingsService, FileSettingsService>();
        services.TryAddSingleton<IDeviceInfo, DefaultDeviceInfo>();
        services.TryAddSingleton<IPlatformDefaults, DefaultPlatformDefaults>();
        services.TryAddSingleton<IPlayerBackend, UnsupportedPlayerBackend>();
        services.TryAddSingleton<IVideoViewFactory, NullVideoViewFactory>();
        services.TryAddSingleton<IFullScreenService, WindowFullScreenService>();
        services.TryAddSingleton<IServerDiscoveryService, ServerDiscoveryService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<INotificationService>(s => s.GetRequiredService<NotificationService>());

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IStateService, StateService>();
        services.AddSingleton<IStateStorageService, StateStorageService>();
        services.AddSingleton<IPreferencesService, PreferencesService>();
        services.AddSingleton<IImageUrlService, ImageUrlService>();
        services.AddSingleton<IImageBlurHashService, ImageBlurHashService>();
        services.AddSingleton<IBackdropService, BackdropService>();
        services.AddSingleton<CardFactory>();
        services.AddSingleton<PlaybackManager>();
        services.AddSingleton<IServerEventService, ServerEventService>();
        services.AddSingleton<RemoteControlService>();

        // Page view models are not registered: NavigationService creates and disposes them.
        services.AddSingleton<MainViewModel>();

        return services;
    }

    private static void AddSdkClients(this IServiceCollection services)
    {
        services.AddHttpClient(HttpClientNames.Default, ConfigureClient)
            .ConfigurePrimaryHttpMessageHandler(CreateHandler);

        services.AddHttpClient(HttpClientNames.Probe, c =>
            {
                ConfigureClient(c);
                c.Timeout = TimeSpan.FromSeconds(8);
            })
            .ConfigurePrimaryHttpMessageHandler(CreateHandler);

        services.AddSingleton<JellyfinSdkSettings>();
        services.AddSingleton<IAuthenticationProvider, JellyfinAuthenticationProvider>();
        services.AddSingleton<IRequestAdapter, JellyfinRequestAdapter>(s => new JellyfinRequestAdapter(
            s.GetRequiredService<IAuthenticationProvider>(),
            s.GetRequiredService<JellyfinSdkSettings>(),
            s.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientNames.Default)));
        services.AddSingleton<JellyfinApiClient>();

        // Subscriptions only exist while a user is signed in, close the connection as soon as they are gone.
        services.AddSingleton(s => new JellyfinWebSocketClient(
            s.GetRequiredService<JellyfinSdkSettings>(),
            new JellyfinWebSocketOptions { IdleTimeout = TimeSpan.Zero }));
    }

    private static void ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(AppConstants.Identifier, AppConstants.Version));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json, 1.0));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
    }

    private static HttpMessageHandler CreateHandler()
    {
        if (OperatingSystem.IsBrowser())
        {
            // The browser handles compression and connections itself.
            return new HttpClientHandler();
        }

        return new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
    }
}
