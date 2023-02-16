using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;
using SystemException = Jellyfin.Sdk.SystemException;

namespace Jellyfin.Mvvm.ViewModels.Login;

/// <summary>
/// The add server view model.
/// </summary>
public partial class AddServerViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly ISystemClient _systemClient;
    private readonly SdkClientSettings _sdkClientSettings;

    [ObservableProperty]
    private string? _serverUrl;

    [ObservableProperty]
    private bool _isValid;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="systemClient">Instance of the <see cref="ISystemClient"/> interface.</param>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    public AddServerViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IStateStorageService stateStorageService,
        ISystemClient systemClient,
        SdkClientSettings sdkClientSettings)
        : base(navigationService, applicationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _systemClient = systemClient;
        _sdkClientSettings = sdkClientSettings;
    }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [RelayCommand]
    private async Task AddServerAsync()
    {
        /* TODO enable after InputKit update.
        if (!IsValid)
        {
            return;
        }
        */

        Loading = true;

        try
        {
            _sdkClientSettings.BaseUrl = ServerUrl;
            _sdkClientSettings.AccessToken = null;

            /*
             * TODO
             * attempt http://domain.tld:8096
             * attempt https://domain.tld:8920
             */

            var publicSystemInfo = await _systemClient.GetPublicSystemInfoAsync()
                .ConfigureAwait(false);

            await _stateStorageService.AddServerAsync(new ServerStateModel
            {
                Id = new Guid(publicSystemInfo.Id),
                Name = publicSystemInfo.ServerName,
                Url = _sdkClientSettings.BaseUrl!
            }).ConfigureAwait(false);

#if DEBUG
            // Mock slow network
            await Task.Delay(1_000).ConfigureAwait(false);
#endif

            _navigationService.NavigateToServerSelectPage();
        }
        catch (SystemException)
        {
            // TODO
        }

        Loading = false;
    }
}
