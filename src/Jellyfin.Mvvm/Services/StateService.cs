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
    }

    /// <inheritdoc />
    public event EventHandler? UserChanged;

    /// <inheritdoc />
    public UserDto? CurrentUser => _state.UserDto;

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        var storeState = await _stateStorageService.GetStoredStateAsync().ConfigureAwait(false);

        var selectedServer = storeState.Servers.FirstOrDefault(x => x.Id == storeState.SelectedServerId);
        if (selectedServer is null)
        {
            return;
        }

        SetServerState(selectedServer);

        var selectedUser = storeState.Users.FirstOrDefault(x => x.Id == storeState.SelectedUserId && x.ServerId == selectedServer.Id);
        if (selectedUser?.Token is null)
        {
            return;
        }

        SetUserState(selectedUser);
        _jellyfinSdkSettings.SetAccessToken(selectedUser.Token);
    }

    /// <inheritdoc />
    public void SetAuthenticationResponse(
        string host,
        AuthenticationResult authenticationResult)
    {
        ArgumentNullException.ThrowIfNull(authenticationResult);

        _jellyfinSdkSettings.SetServerUrl(host);
        _jellyfinSdkSettings.SetAccessToken(authenticationResult.AccessToken);
        _state.Host = host.TrimEnd('/');
        _state.Token = authenticationResult.AccessToken;
        _state.UserDto = authenticationResult.User;
        UserChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SetUser(UserDto userDto)
    {
        _state.UserDto = userDto;
        UserChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public UserDto GetCurrentUser()
    {
        return _state.UserDto ?? throw new UnauthorizedAccessException();
    }

    /// <inheritdoc />
    public void SetServerState(ServerStateModel serverStateModel)
    {
        ArgumentNullException.ThrowIfNull(serverStateModel);

        _state.ServerState = serverStateModel;
        _state.Host = serverStateModel.Url.TrimEnd('/');
        _jellyfinSdkSettings.SetServerUrl(_state.Host);
    }

    /// <inheritdoc />
    public ServerStateModel? GetServerState() => _state.ServerState;

    /// <inheritdoc />
    public UserStateModel? GetUserState() => _state.UserState;

    /// <inheritdoc />
    public void SetUserState(UserStateModel? userStateModel) => _state.UserState = userStateModel;

    /// <inheritdoc />
    public string GetHost() => _state.Host ?? throw new InvalidOperationException("No server is selected");

    /// <inheritdoc />
    public string? GetToken() => _jellyfinSdkSettings.AccessToken;

    /// <inheritdoc />
    public void ClearState()
    {
        _state.UserDto = null;
        _state.Token = null;
        _state.UserState = null;
        UserChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void SetHost(string host)
    {
        _state.Host = host;
    }
}
