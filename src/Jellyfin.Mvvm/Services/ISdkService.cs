namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Jellyfin sdk service.
/// </summary>
public interface ISdkService
{
    /// <summary>
    /// Perform platform-specific initialization.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asyncronous operation.</returns>
    ValueTask InitializeAsync();
}
