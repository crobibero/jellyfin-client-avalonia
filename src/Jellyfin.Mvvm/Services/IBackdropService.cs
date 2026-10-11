namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Controls the backdrop shown behind the content.
/// </summary>
public interface IBackdropService
{
    /// <summary>
    /// Raised when the backdrop changes.
    /// </summary>
    event EventHandler? BackdropChanged;

    /// <summary>
    /// Gets the current backdrop url.
    /// </summary>
    string? BackdropUrl { get; }

    /// <summary>
    /// Set the backdrop.
    /// </summary>
    /// <param name="url">The backdrop url, or <c>null</c> to clear it.</param>
    void SetBackdrop(string? url);
}
