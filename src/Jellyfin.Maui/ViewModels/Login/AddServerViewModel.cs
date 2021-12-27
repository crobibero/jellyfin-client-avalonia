using CommunityToolkit.Mvvm.Input;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;
using Jellyfin.Sdk;
using SystemException = Jellyfin.Sdk.SystemException;

namespace Jellyfin.Maui.ViewModels.Login;

/// <summary>
/// The add server view model.
/// </summary>
public class AddServerViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IStateStorageService _stateStorageService;
    private readonly ISystemClient _systemClient;
    private readonly SdkClientSettings _sdkClientSettings;

    private string? _serverUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    /// <param name="systemClient">Instance of the <see cref="ISystemClient"/> interface.</param>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    public AddServerViewModel(
        INavigationService navigationService,
        IStateStorageService stateStorageService,
        ISystemClient systemClient,
        SdkClientSettings sdkClientSettings)
        : base(navigationService)
    {
        _navigationService = navigationService;
        _stateStorageService = stateStorageService;
        _systemClient = systemClient;
        _sdkClientSettings = sdkClientSettings;

        AddServerCommand = new AsyncRelayCommand(AddServerAsync);
    }

    /// <summary>
    /// Gets or sets the server url.
    /// </summary>
    public string? ServerUrl
    {
        get => _serverUrl;
        set => SetProperty(ref _serverUrl, value);
    }

    /// <summary>
    /// Gets the add server command.
    /// </summary>
    public IAsyncRelayCommand AddServerCommand { get; }

    /// <inheritdoc />
    public override ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async Task AddServerAsync()
    {
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

            var serverId = new Guid(publicSystemInfo.Id);

            var serverStateModel = new ServerStateModel(serverId, publicSystemInfo.ServerName, _sdkClientSettings.BaseUrl!);
            await _stateStorageService.AddServerAsync(serverStateModel).ConfigureAwait(false);
            _navigationService.NavigateToServerSelectPage();
        }
        catch (SystemException)
        {
            // TODO
        }
    }
}
