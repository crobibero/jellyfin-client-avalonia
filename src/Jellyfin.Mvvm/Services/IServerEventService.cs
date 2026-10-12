namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Receives messages the server pushes over the WebSocket while a user is signed in.
/// </summary>
/// <remarks>
/// Events are raised on the UI thread.
/// </remarks>
public interface IServerEventService
{
    /// <summary>
    /// Raised when the played, favorite or progress state of items changed.
    /// </summary>
    event EventHandler<UserDataChangeInfo>? UserDataChanged;

    /// <summary>
    /// Raised when items were added to, updated in or removed from the library.
    /// </summary>
    event EventHandler<LibraryUpdateInfo>? LibraryChanged;

    /// <summary>
    /// Raised when a remote control asks this client to play items.
    /// </summary>
    event EventHandler<PlayRequest>? PlayRequested;

    /// <summary>
    /// Raised when a remote control sends a playback command, such as pause or seek.
    /// </summary>
    event EventHandler<PlaystateRequest>? PlaystateRequested;

    /// <summary>
    /// Raised when a remote control sends a general command, such as a volume change.
    /// </summary>
    event EventHandler<GeneralCommand>? GeneralCommandReceived;

    /// <summary>
    /// Start following the signed in user. The connection is open while a user is signed in.
    /// </summary>
    void Start();
}
