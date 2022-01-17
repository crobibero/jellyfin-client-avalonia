using Jellyfin.Maui.Models;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class StateService : IStateService
{
    private readonly CurrentStateModel _state;
    private readonly SdkClientSettings _sdkClientSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateService"/> class.
    /// </summary>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    public StateService(SdkClientSettings sdkClientSettings)
    {
        _sdkClientSettings = sdkClientSettings;
        // TODO load from disk
        _state = new CurrentStateModel();
        _sdkClientSettings.BaseUrl = _state.Host;
        _sdkClientSettings.AccessToken = _state.Token;
    }

    /// <inheritdoc />
    public void SetAuthenticationResponse(
        string host,
        AuthenticationResult authenticationResult)
    {
        _sdkClientSettings.BaseUrl = host;
        _sdkClientSettings.AccessToken = authenticationResult.AccessToken;
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
        _sdkClientSettings.BaseUrl = serverStateModel.Url;
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
    public string GetToken()
    {
        return _sdkClientSettings.AccessToken;
    }

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
