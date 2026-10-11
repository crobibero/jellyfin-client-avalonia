namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A server that was successfully contacted.
/// </summary>
/// <param name="Url">The resolved server url.</param>
/// <param name="Id">The server id.</param>
/// <param name="Name">The server name.</param>
/// <param name="Version">The server version.</param>
public sealed record ServerInfoModel(string Url, Guid Id, string Name, string? Version);
