using System.Text.Json;
using System.Text.Json.Serialization;
using Jellyfin.Mvvm.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public partial class AuthenticationService : IAuthenticationService
{
    private static readonly TimeSpan _probeTimeout = TimeSpan.FromSeconds(8);

    private readonly JellyfinApiClient _jellyfinApiClient;
    private readonly IStateService _stateService;
    private readonly JellyfinSdkSettings _jellyfinSdkSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthenticationService> _logger;

    private string? _quickConnectSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="jellyfinApiClient">The Jellyfin api client.</param>
    /// <param name="jellyfinSdkSettings">The Jellyfin sdk settings.</param>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="logger">The logger.</param>
    public AuthenticationService(
        IStateService stateService,
        JellyfinApiClient jellyfinApiClient,
        JellyfinSdkSettings jellyfinSdkSettings,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthenticationService> logger)
    {
        _stateService = stateService;
        _jellyfinApiClient = jellyfinApiClient;
        _jellyfinSdkSettings = jellyfinSdkSettings;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<ServerInfoModel?> ResolveServerAsync(string address, CancellationToken cancellationToken = default)
    {
        var candidates = GetAddressCandidates(address);
        if (candidates.Count == 0)
        {
            return null;
        }

        using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationSource.CancelAfter(_probeTimeout);

        var probes = candidates.Select(c => ProbeAsync(c, cancellationSource.Token)).ToList();

        // Candidates are ordered by preference, use the first that responds and stop the others.
        foreach (var probe in probes)
        {
            var result = await probe.ConfigureAwait(false);
            if (result is not null)
            {
                await cancellationSource.CancelAsync().ConfigureAwait(false);
                return result;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<UserDto>> GetPublicUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _jellyfinApiClient.Users.Public.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to get public users");
            return [];
        }
    }

    /// <inheritdoc />
    public async ValueTask<(bool Status, string? ErrorMessage)> AuthenticateAsync(
        string host,
        string username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        _jellyfinSdkSettings.SetServerUrl(host);
        _jellyfinSdkSettings.SetAccessToken(null);

        try
        {
            _ = await _jellyfinApiClient.System.Info.Public.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to connect to {Host}", host);
            return (false, Strings.Strings.Login_UnableToConnect);
        }

        try
        {
            var authResult = await _jellyfinApiClient.Users.AuthenticateByName.PostAsync(
                    new AuthenticateUserByName { Username = username, Pw = password ?? string.Empty },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (authResult is null)
            {
                return (false, string.Format(System.Globalization.CultureInfo.CurrentCulture, Strings.Strings.Login_UnknownError, string.Empty));
            }

            _stateService.SetAuthenticationResponse(host, authResult);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Authentication failed for {Username}", username);
            return (false, Strings.Strings.Login_InvalidCredentials);
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to initiate Quick Connect");
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
        catch (OperationCanceledException)
        {
            throw;
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
                return (false, Strings.Strings.Login_QuickConnectUnavailable);
            }

            _stateService.SetAuthenticationResponse(_stateService.GetHost(), response);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Quick Connect authentication failed");
            return (false, Strings.Strings.Login_QuickConnectUnavailable);
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
    public async ValueTask LogoutAsync(bool revokeToken = false)
    {
        if (revokeToken && !string.IsNullOrEmpty(_jellyfinSdkSettings.AccessToken))
        {
            try
            {
                await _jellyfinApiClient.Sessions.Logout.PostAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Unable to revoke the session");
            }
        }

        _jellyfinSdkSettings.SetAccessToken(null);
        _stateService.ClearState();
    }

    /// <inheritdoc />
    public ValueTask<bool> IsAuthenticatedAsync(string host, string accessToken, CancellationToken cancellationToken = default)
    {
        _stateService.SetHost(host);
        _jellyfinSdkSettings.SetServerUrl(host);
        _jellyfinSdkSettings.SetAccessToken(accessToken);
        return IsAuthenticatedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_jellyfinSdkSettings.ServerUrl) || string.IsNullOrEmpty(_jellyfinSdkSettings.AccessToken))
        {
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
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Stored session is no longer valid");
            _jellyfinSdkSettings.SetAccessToken(null);
            return false;
        }
    }

    internal static IReadOnlyList<string> GetAddressCandidates(string? address)
    {
        address = address?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(address))
        {
            return [];
        }

        var hasScheme = address.Contains("://", StringComparison.Ordinal);
        if (!Uri.TryCreate(hasScheme ? address : "http://" + address, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return [];
        }

        var path = uri.AbsolutePath.TrimEnd('/');
        var host = uri.IsDefaultPort ? uri.Host : uri.Authority;
        var candidates = new List<string>();

        void Add(string candidate)
        {
            if (!candidates.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(candidate);
            }
        }

        if (hasScheme)
        {
            // Respect an explicit scheme.
            Add($"{uri.Scheme}://{host}{path}");
        }
        else if (!uri.IsDefaultPort)
        {
            Add($"https://{host}{path}");
            Add($"http://{host}{path}");
        }
        else
        {
            Add($"https://{host}{path}");
            Add($"http://{host}:8096{path}");
            Add($"https://{host}:8920{path}");
            Add($"http://{host}{path}");
        }

        return candidates;
    }

    private async Task<ServerInfoModel?> ProbeAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientNames.Probe);
            using var response = await client.GetAsync(new Uri(url + "/System/Info/Public"), cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using (stream.ConfigureAwait(false))
            {
                var info = await JsonSerializer.DeserializeAsync(stream, ProbeJsonContext.Default.PublicServerInfo, cancellationToken).ConfigureAwait(false);
                if (info?.Id is null || !Guid.TryParse(info.Id, out var id))
                {
                    return null;
                }

                return new ServerInfoModel(url, id, info.ServerName ?? url, info.Version);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or UriFormatException)
        {
            _logger.LogDebug(ex, "Probe of {Url} failed", url);
            return null;
        }
    }

    /// <summary>
    /// The public system information returned by the server.
    /// </summary>
    internal sealed record PublicServerInfo
    {
        /// <summary>
        /// Gets the server id.
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// Gets the server name.
        /// </summary>
        public string? ServerName { get; init; }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        public string? Version { get; init; }
    }

    [JsonSerializable(typeof(PublicServerInfo))]
    internal sealed partial class ProbeJsonContext : JsonSerializerContext
    {
    }
}
