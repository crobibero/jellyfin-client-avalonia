using Jellyfin.Mvvm.Models;


namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class StateService : IStateService
{
    private readonly CurrentStateModel _state;
    private readonly JellyfinSdkSettings _jellyfinSdkSettings;
    private readonly IStateStorageService _stateStorageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateService"/> class.
    /// </summary>
    /// <param name="jellyfinSdkSettings">Instance of the <see cref="JellyfinSdkSettings"/>.</param>
    /// <param name="stateStorageService">Instance of the <see cref="IStateStorageService"/> interface.</param>
    public StateService(JellyfinSdkSettings jellyfinSdkSettings, IStateStorageService stateStorageService)
    {
        _jellyfinSdkSettings = jellyfinSdkSettings;
        _stateStorageService = stateStorageService;

        _state = new CurrentStateModel();
        _jellyfinSdkSettings.SetServerUrl(_state.Host);
        _jellyfinSdkSettings.SetAccessToken(_state.Token);
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        var storeState = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);

        if (storeState.SelectedServerId is null
            || storeState.SelectedUserId is null)
        {
            return;
        }

        var selectedServer = storeState.Servers.FirstOrDefault(x => x.Id == storeState.SelectedServerId);

        if (selectedServer is null)
        {
            return;
        }

        var selectedUser = storeState.Users.FirstOrDefault(x => x.Id == storeState.SelectedUserId && x.ServerId == selectedServer.Id);

        if (selectedUser is null)
        {
            return;
        }

        SetServerState(selectedServer);
        SetUserState(selectedUser);

        _jellyfinSdkSettings.SetServerUrl(selectedServer.Url);
        _jellyfinSdkSettings.SetAccessToken(selectedUser.Token);
    }

    /// <inheritdoc />
    public void SetAuthenticationResponse(
        string host,
        AuthenticationResult authenticationResult)
    {
        _jellyfinSdkSettings.SetServerUrl(host);
        _jellyfinSdkSettings.SetAccessToken(authenticationResult.AccessToken);
        _state.Host = host.TrimEnd('/');
        _state.UserDto = authenticationResult.User;
        _state.Token = authenticationResult.AccessToken;
        CurrentStateModel.StaticHost = _state.Host;
    }

    /// <inheritdoc />
    public void SetUser(UserDto userDto)
    {
        _state.UserDto = userDto;
    }

    /// <inheritdoc />
    public UserDto GetCurrentUser()
    {
        return _state.UserDto ?? throw new UnauthorizedAccessException();
    }

    /// <inheritdoc />
    public void SetServerState(ServerStateModel serverStateModel)
    {
        _state.ServerState = serverStateModel;
        _state.Host = serverStateModel.Url;
        _jellyfinSdkSettings.SetServerUrl(serverStateModel.Url);
        CurrentStateModel.StaticHost = serverStateModel.Url;
    }

    /// <inheritdoc />
    public ServerStateModel? GetServerState()
    {
        return _state.ServerState;
    }

    /// <inheritdoc />
    public UserStateModel? GetUserState()
    {
        return _state.UserState;
    }

    /// <inheritdoc />
    public void SetUserState(UserStateModel userStateModel)
    {
        _state.UserState = userStateModel;
    }

    /// <inheritdoc />
    public string GetHost()
    {
        return _state.Host ?? throw new InvalidOperationException();
    }

    /// <inheritdoc />
    public string? GetToken()
        => _jellyfinSdkSettings.AccessToken;

    /// <inheritdoc />
    public void ClearState()
    {
        _state.UserDto = null;
        _state.Host = null;
        _state.Token = null;
        CurrentStateModel.StaticHost = null;
    }

    /// <inheritdoc/>
    public void SetHost(string host)
    {
        _state.Host = host;
        CurrentStateModel.StaticHost = host;
    }
}
