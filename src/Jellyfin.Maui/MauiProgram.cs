using System.Globalization;
using System.Net;
using System.Text;
using CommunityToolkit.Maui;
using InputKit.Handlers;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Pages.Login;
using Jellyfin.Maui.Services;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels;
using Jellyfin.Mvvm.ViewModels.Login;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using UraniumUI;

namespace Jellyfin.Maui;

/// <summary>
/// The main maui program.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Create the maui app.
    /// </summary>
    /// <param name="useShellNavigation">whether or not should the app use shell navigation.</param>
    /// <returns>The created maui app.</returns>
    public static MauiApp CreateMauiApp(bool useShellNavigation = true)
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Quicksand-Bold.ttf", "QuicksandBold");
                fonts.AddFont("Quicksand-Light.ttf", "QuicksandLight");
                fonts.AddFont("Quicksand-Medium.ttf", "QuicksandMedium");
                fonts.AddFont("Quicksand-Regular.ttf", "QuicksandRegular");
                fonts.AddFont("Quicksand-SemiBold.ttf", "QuicksandSemiBold");
                fonts.AddMaterialIconFonts();
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddInputKitHandlers();
            });

        BuildLogger();
        builder.Services.AddPages(useShellNavigation);
        builder.Services.AddSdkClients();
        builder.Services.AddServices(useShellNavigation);
        builder.Logging.AddSerilog();
        return builder.Build();
    }

    private static void AddPages(this IServiceCollection services, bool useShellNavigation)
    {
        // Login Flow doesn't use shell navigation.
        services.AddTransient<AddServerPage, AddServerViewModel>();
        services.AddTransient<LoginPage, LoginViewModel>();
        services.AddTransient<SelectServerPage, ServerSelectViewModel>();
        services.AddTransient<SelectUserPage, SelectUserViewModel>();

        // HomePage is automatically registered if using shell navigation.
        services.AddTransient<HomePage, HomeViewModel>();
        services.AddTransient<LoadingPage>();

        if (useShellNavigation)
        {
            services.AddTransientWithShellRoute<ItemPage, ItemViewModel>(nameof(ItemPage));
            services.AddTransientWithShellRoute<LibraryPage, LibraryViewModel>(nameof(LibraryViewModel));
        }
        else
        {
            services.AddTransient<ItemPage, ItemViewModel>();
            services.AddTransient<LibraryPage, LibraryViewModel>();
        }
    }

    private static void AddServices(this IServiceCollection services, bool useShellNavigation)
    {
        services.AddLocalization();
        services.AddSingleton(Microsoft.Maui.Devices.DeviceInfo.Current);
        services.AddSingleton(SecureStorage.Default);
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddSingleton<IStateService, StateService>();
        if (useShellNavigation)
        {
            services.AddSingleton<INavigationService, ShellNavigationService>();
        }
        else
        {
            services.AddSingleton<INavigationService, NavigationService>();
        }

        services.AddSingleton<IApplicationService, ApplicationService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IStateStorageService, StateStorageService>();
        services.AddSingleton<ISdkService, SdkService>();
    }

    private static void AddSdkClients(this IServiceCollection services)
    {
        static HttpMessageHandler DefaultHttpClientHandlerDelegate(IServiceProvider serviceProvider)
        {
            return new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8
            };
        }

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

        // Register sdk services
        services.AddSingleton<SdkClientSettings>();

        // TODO remove unused clients.
        services.AddHttpClient<IApiKeyClient, ApiKeyClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IArtistsClient, ArtistsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IAudioClient, AudioClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IBrandingClient, BrandingClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IChannelsClient, ChannelsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ICollectionClient, CollectionClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IConfigurationClient, ConfigurationClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDashboardClient, DashboardClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDevicesClient, DevicesClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDisplayPreferencesClient, DisplayPreferencesClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDlnaClient, DlnaClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDlnaServerClient, DlnaServerClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IDynamicHlsClient, DynamicHlsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IEnvironmentClient, EnvironmentClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IFilterClient, FilterClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IGenresClient, GenresClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IHlsSegmentClient, HlsSegmentClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IImageClient, ImageClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IImageByNameClient, ImageByNameClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IInstantMixClient, InstantMixClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IItemLookupClient, ItemLookupClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IItemRefreshClient, ItemRefreshClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IItemsClient, ItemsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ILibraryClient, LibraryClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IItemUpdateClient, ItemUpdateClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ILibraryStructureClient, LibraryStructureClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ILiveTvClient, LiveTvClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ILocalizationClient, LocalizationClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IMediaInfoClient, MediaInfoClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IMoviesClient, MoviesClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IMusicGenresClient, MusicGenresClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<INotificationsClient, NotificationsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IPackageClient, PackageClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IPersonsClient, PersonsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IPlaylistsClient, PlaylistsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IPlaystateClient, PlaystateClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IPluginsClient, PluginsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IQuickConnectClient, QuickConnectClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IRemoteImageClient, RemoteImageClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IScheduledTasksClient, ScheduledTasksClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ISearchClient, SearchClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ISessionClient, SessionClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IStartupClient, StartupClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IStudiosClient, StudiosClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ISubtitleClient, SubtitleClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ISuggestionsClient, SuggestionsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ISyncPlayClient, SyncPlayClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ISystemClient, SystemClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ITimeSyncClient, TimeSyncClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ITrailersClient, TrailersClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ITvShowsClient, TvShowsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IUniversalAudioClient, UniversalAudioClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IUserClient, UserClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IUserLibraryClient, UserLibraryClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IUserViewsClient, UserViewsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IVideoAttachmentsClient, VideoAttachmentsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IVideosClient, VideosClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IYearsClient, YearsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate)
            .AddPolicyHandler(retryPolicy);
    }

    private static void BuildLogger()
    {
        const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {SourceContext}: {Message}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
#if IOS || MACCATALYST
            .WriteTo.NSLog(
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture)
#elif WINDOWS
#if DEBUG
            .WriteTo.Debug(
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture)
#endif
            /* Path is %LocalAppData%/Packages/org.jellyfin.maui_{unique} */
            .WriteTo.File(
                path: Path.Combine(FileSystem.AppDataDirectory, "Jellyfin.Maui..log"),
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                encoding: Encoding.UTF8,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10)
#endif
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
