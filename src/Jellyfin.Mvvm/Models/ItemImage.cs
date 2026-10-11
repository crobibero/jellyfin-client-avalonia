namespace Jellyfin.Mvvm.Models;

/// <summary>
/// An image url and the blurhash shown while it loads.
/// </summary>
/// <param name="Url">The image url.</param>
/// <param name="BlurHash">The blurhash, or <c>null</c> when the server has none.</param>
public sealed record ItemImage(string Url, string? BlurHash);
