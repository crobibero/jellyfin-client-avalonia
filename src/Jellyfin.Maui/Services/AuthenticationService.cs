using SystemException = Jellyfin.Sdk.SystemException;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserClient _userClient;
    private readonly ISystemClient _systemClient;
    private readonly IQuickConnectClient _quickConnectClient;

    private readonly IStateService _stateService;
    private readonly SdkClientSettings _sdkClientSettings;

    private string? _quickConnectSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="userClient">Instance of the <see cref="IUserClient"/> interface.</param>
    /// <param name="systemClient">Instance of the <see cref="ISystemClient"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="sdkClientSettings">Instance of the <see cref="SdkClientSettings"/>.</param>
    /// <param name="quickConnectClient">Instance of the <see cref="IQuickConnectClient"/> interface.</param>
    public AuthenticationService(
        IUserClient userClient,
        ISystemClient systemClient,
        IStateService stateService,
        SdkClientSettings sdkClientSettings,
        IQuickConnectClient quickConnectClient)
    {
        _userClient = userClient;
        _systemClient = systemClient;
        _stateService = stateService;
        _sdkClientSettings = sdkClientSettings;
        _quickConnectClient = quickConnectClient;
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
    public async ValueTask<string?> InitializeQuickConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _quickConnectClient.InitiateAsync(cancellationToken).ConfigureAwait(false);
            _quickConnectSecret = response?.Secret;
            return response?.Code;
        }
        catch (QuickConnectException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool?> TestQuickConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _quickConnectClient.ConnectAsync(_quickConnectSecret, cancellationToken)
                .ConfigureAwait(false);
            return result?.Authenticated;
        }
        catch (QuickConnectException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async ValueTask<(bool Status, string? ErrorMessage)> AuthenticateWithQuickConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _userClient.AuthenticateWithQuickConnectAsync(new QuickConnectDto { Secret = _quickConnectSecret }, cancellationToken)
                .ConfigureAwait(false);
            if (response is null)
            {
                return (false, null);
            }

            _stateService.SetAuthenticationResponse(_stateService.GetHost(), response);
            return (true, null);
        }
        catch (UserException)
        {
            return (false, null);
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsQuickConnectEnabledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _quickConnectClient.GetEnabledAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (QuickConnectException)
        {
            return false;
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
    public ValueTask<bool> IsAuthenticatedAsync(string host, string accessToken, CancellationToken cancellationToken = default)
    {
        _stateService.SetHost(host);
        _sdkClientSettings.BaseUrl = host;
        _sdkClientSettings.AccessToken = accessToken;
        return IsAuthenticatedAsync(cancellationToken);
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
            _sdkClientSettings.AccessToken = null;
            return false;
        }
    }
}
