using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// The add server view model.
/// </summary>
public partial class AddServerViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly JellyfinApiClient _jellyfinApiClient;
    private readonly JellyfinSdkSettings _jellyfinSdkSettings;

    [ObservableProperty]
    private string? _serverUrl;

    [ObservableProperty]
    private bool _isValid = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="jellyfinApiClient">The Jellyfin api client.</param>
    /// <param name="jellyfinSdkSettings">The Jellyfin sdk settings.</param>
    public AddServerViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        JellyfinApiClient jellyfinApiClient,
        JellyfinSdkSettings jellyfinSdkSettings)
        : base(navigationService, applicationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _jellyfinApiClient = jellyfinApiClient;
        _jellyfinSdkSettings = jellyfinSdkSettings;
    }

    /// <inheritdoc />
    protected override ValueTask InitializeInternalAsync()
    {
        return ValueTask.CompletedTask;
    }

    [RelayCommand]
    private async Task AddServerAsync()
    {
        if (!IsValid)
        {
            return;
        }

        Loading = true;

        try
        {
            _jellyfinSdkSettings.SetServerUrl(ServerUrl);
            _jellyfinSdkSettings.SetAccessToken(null);

            /*
             * TODO
             * attempt http://domain.tld:8096
             * attempt https://domain.tld:8920
             */

            var publicSystemInfo = await _jellyfinApiClient.System.Info.Public.GetAsync()
                .ConfigureAwait(false);

            if (publicSystemInfo is null)
            {
                return;
            }

            await _stateStorageService.AddServerAsync(new ServerStateModel
            {
                Id = new Guid(publicSystemInfo.Id!),
                Name = publicSystemInfo.ServerName!,
                Url = _jellyfinSdkSettings.ServerUrl!
            }).ConfigureAwait(false);

#if DEBUG
            // Mock slow network
            await Task.Delay(1_000).ConfigureAwait(false);
#endif

            _navigationService.NavigateToServerSelectPage();
        }
        catch (InvalidOperationException)
        {
            // TODO
        }
        catch (SystemException)
        {
            // TODO
        }

        Loading = false;
    }
}
