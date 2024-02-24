

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class AuthenticationService : IAuthenticationService
{
    private readonly JellyfinApiClient _jellyfinApiClient;

    private readonly IStateService _stateService;
    private readonly JellyfinSdkSettings _jellyfinSdkSettings;

    private string? _quickConnectSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="jellyfinApiClient">The Jellyfin api client.</param>
    /// <param name="jellyfinSdkSettings">The Jellyfin sdk settings.</param>
    public AuthenticationService(
        IStateService stateService,
        JellyfinApiClient jellyfinApiClient,
        JellyfinSdkSettings jellyfinSdkSettings)
    {
        _stateService = stateService;
        _jellyfinApiClient = jellyfinApiClient;
        _jellyfinSdkSettings = jellyfinSdkSettings;
    }

    /// <inheritdoc />
    public async ValueTask<(bool Status, string? ErrorMessage)> AuthenticateAsync(
        string host,
        string username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        // Set baseurl.
        _jellyfinSdkSettings.SetServerUrl(host);
        _jellyfinApiClient.Update();

        // Unset access token.
        _jellyfinSdkSettings.SetAccessToken(null);

        try
        {
            _ = await _jellyfinApiClient.System.Info.Public.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return (false, "Unable to connect to server");
        }

        try
        {
            var authResult = await _jellyfinApiClient.Users.AuthenticateByName.PostAsync(
                    new AuthenticateUserByName { Username = username, Pw = password },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (authResult is null)
            {
                return (false, "Unknown error");
            }

            _stateService.SetAuthenticationResponse(host, authResult);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, "Invalid username or password");
        }
    }

    /// <inheritdoc />
    public async ValueTask<string?> InitializeQuickConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _jellyfinApiClient.QuickConnect.Initiate.PostAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            _quickConnectSecret = response?.Secret;
            return response?.Code;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool?> TestQuickConnectAsync(CancellationToken cancellationToken)
    {
        try
        {

            var result = await _jellyfinApiClient.QuickConnect.Connect.GetAsync(
                    c => c.QueryParameters.Secret = _quickConnectSecret,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return result?.Authenticated;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async ValueTask<(bool Status, string? ErrorMessage)> AuthenticateWithQuickConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _jellyfinApiClient.Users.AuthenticateWithQuickConnect.PostAsync(
                    new QuickConnectDto { Secret = _quickConnectSecret },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (response is null)
            {
                return (false, null);
            }

            _stateService.SetAuthenticationResponse(_stateService.GetHost(), response);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsQuickConnectEnabledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _jellyfinApiClient.QuickConnect.Enabled.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false) ?? false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public void Logout()
    {
        _jellyfinSdkSettings.SetServerUrl(null);
        _jellyfinApiClient.Update();
        _jellyfinSdkSettings.SetAccessToken(null);
        _stateService.ClearState();
    }

    /// <inheritdoc />
    public ValueTask<bool> IsAuthenticatedAsync(string host, string accessToken, CancellationToken cancellationToken = default)
    {
        _stateService.SetHost(host);
        _jellyfinSdkSettings.SetServerUrl(host);
        _jellyfinApiClient.Update();
        _jellyfinSdkSettings.SetAccessToken(accessToken);
        return IsAuthenticatedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_jellyfinSdkSettings.ServerUrl))
        {
            _jellyfinSdkSettings.SetAccessToken(null);
            return false;
        }

        try
        {
            var user = await _jellyfinApiClient.Users.Me.GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                return false;
            }

            _stateService.SetUser(user);
            return true;
        }
        catch (Exception)
        {
            _jellyfinSdkSettings.SetAccessToken(null);
            return false;
        }
    }
}
