using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Jellyfin.Mvvm.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Discovers servers with the Jellyfin UDP discovery protocol.
/// </summary>
public class ServerDiscoveryService : IServerDiscoveryService
{
    private const int DiscoveryPort = 7359;
    private static readonly byte[] _discoveryMessage = Encoding.UTF8.GetBytes("who is JellyfinServer?");

    private readonly IDeviceInfo _deviceInfo;
    private readonly ILogger<ServerDiscoveryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerDiscoveryService"/> class.
    /// </summary>
    /// <param name="deviceInfo">The device info.</param>
    /// <param name="logger">The logger.</param>
    public ServerDiscoveryService(IDeviceInfo deviceInfo, ILogger<ServerDiscoveryService> logger)
    {
        _deviceInfo = deviceInfo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServerDiscoveryResponse> DiscoverAsync(TimeSpan timeout, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_deviceInfo.SupportsServerDiscovery)
        {
            yield break;
        }

        UdpClient? client = null;
        try
        {
            client = new UdpClient { EnableBroadcast = true };
            await client.SendAsync(_discoveryMessage, new IPEndPoint(IPAddress.Broadcast, DiscoveryPort), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is SocketException or PlatformNotSupportedException or OperationCanceledException)
        {
            _logger.LogInformation(ex, "Server discovery is unavailable");
            client?.Dispose();
            yield break;
        }

        using (client)
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (!timeoutSource.IsCancellationRequested)
            {
                ServerDiscoveryResponse? response;
                try
                {
                    var result = await client.ReceiveAsync(timeoutSource.Token).ConfigureAwait(false);
                    response = JsonSerializer.Deserialize(result.Buffer, JsonOptions.Context.ServerDiscoveryResponse);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is SocketException or JsonException)
                {
                    _logger.LogDebug(ex, "Invalid discovery response");
                    continue;
                }

                if (response?.Address is not null && seen.Add(response.Id ?? response.Address))
                {
                    yield return response;
                }
            }
        }
    }
}
