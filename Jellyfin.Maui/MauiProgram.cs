using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using System.Net;
using System.Text;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.Pages;
using Jellyfin.Maui.Services;

namespace Jellyfin.Maui
{
    /// <summary>
    /// The main maui program.
    /// </summary>
    public static class MauiProgram
	{
        /// <summary>
        /// Create the maui app.
        /// </summary>
        /// <returns>The created maui app.</returns>
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

            builder.Services.AddPages();
            builder.Services.AddSdkClients();
            builder.Services.AddServices();           

            return builder.Build();
		}

        private static void AddPages(this IServiceCollection services)
        {
            var exportedTypes = typeof(MauiProgram).Assembly.GetTypes();
            var baseViewModelType = typeof(BaseViewModel);
            var basePageType = typeof(BaseContentPage<>);
            var contentPageType = typeof(Microsoft.Maui.Controls.ContentPage);

            foreach (var type in exportedTypes)
            {
                if (type != baseViewModelType && baseViewModelType.IsAssignableFrom(type))
                {
                    // Add View Models
                    services.AddTransient(type);
                }

                if (type != basePageType && contentPageType.IsAssignableFrom(type))
                {
                    // Add Pages
                    services.AddTransient(type);
                }
            }
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IStateService, StateService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<ILibraryService, LibraryService>();
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

            // Register sdk services
            services.AddSingleton<SdkClientSettings>();
            services.AddHttpClient<IActivityLogClient, ActivityLogClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IApiKeyClient, ApiKeyClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IArtistsClient, ArtistsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IAudioClient, AudioClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IBrandingClient, BrandingClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IChannelsClient, ChannelsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ICollectionClient, CollectionClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IConfigurationClient, ConfigurationClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IDashboardClient, DashboardClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IDevicesClient, DevicesClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IDisplayPreferencesClient, DisplayPreferencesClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IDlnaClient, DlnaClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IDlnaServerClient, DlnaServerClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IDynamicHlsClient, DynamicHlsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IEnvironmentClient, EnvironmentClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IFilterClient, FilterClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IGenresClient, GenresClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IHlsSegmentClient, HlsSegmentClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IImageClient, ImageClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IImageByNameClient, ImageByNameClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IInstantMixClient, InstantMixClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IItemLookupClient, ItemLookupClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IItemRefreshClient, ItemRefreshClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IItemsClient, ItemsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ILibraryClient, LibraryClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IItemUpdateClient, ItemUpdateClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ILibraryStructureClient, LibraryStructureClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ILiveTvClient, LiveTvClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ILocalizationClient, LocalizationClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IMediaInfoClient, MediaInfoClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IMoviesClient, MoviesClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IMusicGenresClient, MusicGenresClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<INotificationsClient, NotificationsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IPackageClient, PackageClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IPersonsClient, PersonsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IPlaylistsClient, PlaylistsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IPlaystateClient, PlaystateClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IPluginsClient, PluginsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IQuickConnectClient, QuickConnectClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IRemoteImageClient, RemoteImageClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IScheduledTasksClient, ScheduledTasksClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ISearchClient, SearchClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ISessionClient, SessionClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IStartupClient, StartupClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IStudiosClient, StudiosClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ISubtitleClient, SubtitleClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ISuggestionsClient, SuggestionsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ISyncPlayClient, SyncPlayClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ISystemClient, SystemClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ITimeSyncClient, TimeSyncClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ITrailersClient, TrailersClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<ITvShowsClient, TvShowsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IUniversalAudioClient, UniversalAudioClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IUserClient, UserClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IUserLibraryClient, UserLibraryClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IUserViewsClient, UserViewsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IVideoAttachmentsClient, VideoAttachmentsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IVideoHlsClient, VideoHlsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IVideosClient, VideosClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            services.AddHttpClient<IYearsClient, YearsClient>()
                .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
        }
	}
}