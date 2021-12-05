using Jellyfin.Sdk;
using SystemException = Jellyfin.Sdk.SystemException;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserClient _userClient;
    private readonly ISystemClient _systemClient;

    private readonly IStateService _stateService;
    private readonly SdkClientSettings _sdkClientSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="userClient">Instance of the <see cref="IUserClient"/> interface.</param>
    /// <param name="systemClient">Instance of the <see cref="ISystemClient"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    public AuthenticationService(
        IUserClient userClient,
        ISystemClient systemClient,
        IStateService stateService,
        SdkClientSettings sdkClientSettings)
    {
        _userClient = userClient;
        _systemClient = systemClient;
        _stateService = stateService;
        _sdkClientSettings = sdkClientSettings;
    }

    /// <inheritdoc />
    public async ValueTask<(bool Status, string? ErrorMessage)> AuthenticateAsync(
        string host,
        string username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        // Set baseurl.
        _sdkClientSettings.BaseUrl = host;

        // Unset access token.
        _sdkClientSettings.AccessToken = null;

        try
        {
            _ = await _systemClient.GetPublicSystemInfoAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (SystemException)
        {
            return (false, "Unable to connect to server");
        }

        try
        {
            var authResult = await _userClient.AuthenticateUserByNameAsync(
                    new AuthenticateUserByName { Username = username, Pw = password },
                    cancellationToken)
                .ConfigureAwait(false);

            _stateService.SetAuthenticationResponse(host, authResult);
            return (true, null);
        }
        catch (UserException)
        {
            return (false, "Invalid username or password");
        }
    }

    /// <inheritdoc />
    public void Logout()
    {
        _sdkClientSettings.BaseUrl = null;
        _sdkClientSettings.AccessToken = null;
        _stateService.ClearState();
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userClient.GetCurrentUserAsync(cancellationToken)
                .ConfigureAwait(false);
            _stateService.SetUser(user);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
