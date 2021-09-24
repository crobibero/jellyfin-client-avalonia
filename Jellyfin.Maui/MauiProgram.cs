using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.Views;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Jellyfin.Maui
{
    /// <summary>
    /// The main entrypoint.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates the maui application.
        /// </summary>
        /// <returns>The created application.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            static HttpMessageHandler DefaultHttpClientHandlerDelegate(IServiceProvider serviceProvider)
            {
                return new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8
                };
            }

            // View Models
            builder.Services.AddTransient<LoginViewModel>();

            // Views
            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<Page2>();

            // Services
            builder.Services.AddSingleton<IStateService, StateService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<ILibraryService, LibraryService>();

            // Register sdk services
            builder.Services.AddSingleton<SdkClientSettings>();
            builder.Services.AddHttpClient<IActivityLogClient, ActivityLogClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IApiKeyClient, ApiKeyClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IArtistsClient, ArtistsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IAudioClient, AudioClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IBrandingClient, BrandingClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IChannelsClient, ChannelsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ICollectionClient, CollectionClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IConfigurationClient, ConfigurationClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IDashboardClient, DashboardClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IDevicesClient, DevicesClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IDisplayPreferencesClient, DisplayPreferencesClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IDlnaClient, DlnaClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IDlnaServerClient, DlnaServerClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IDynamicHlsClient, DynamicHlsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IEnvironmentClient, EnvironmentClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IFilterClient, FilterClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IGenresClient, GenresClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IHlsSegmentClient, HlsSegmentClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IImageClient, ImageClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IImageByNameClient, ImageByNameClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IInstantMixClient, InstantMixClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IItemLookupClient, ItemLookupClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IItemRefreshClient, ItemRefreshClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IItemsClient, ItemsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ILibraryClient, LibraryClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IItemUpdateClient, ItemUpdateClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ILibraryStructureClient, LibraryStructureClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ILiveTvClient, LiveTvClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ILocalizationClient, LocalizationClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IMediaInfoClient, MediaInfoClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IMoviesClient, MoviesClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IMusicGenresClient, MusicGenresClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<INotificationsClient, NotificationsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IPackageClient, PackageClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IPersonsClient, PersonsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IPlaylistsClient, PlaylistsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IPlaystateClient, PlaystateClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IPluginsClient, PluginsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IQuickConnectClient, QuickConnectClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IRemoteImageClient, RemoteImageClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IScheduledTasksClient, ScheduledTasksClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ISearchClient, SearchClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ISessionClient, SessionClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IStartupClient, StartupClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IStudiosClient, StudiosClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ISubtitleClient, SubtitleClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ISuggestionsClient, SuggestionsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ISyncPlayClient, SyncPlayClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ISystemClient, SystemClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ITimeSyncClient, TimeSyncClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ITrailersClient, TrailersClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<ITvShowsClient, TvShowsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IUniversalAudioClient, UniversalAudioClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IUserClient, UserClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IUserLibraryClient, UserLibraryClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IUserViewsClient, UserViewsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IVideoAttachmentsClient, VideoAttachmentsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IVideoHlsClient, VideoHlsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IVideosClient, VideosClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            builder.Services.AddHttpClient<IYearsClient, YearsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);

            return builder.Build();
        }
    }
}