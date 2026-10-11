namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Shows transient messages to the user.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Show a message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="isError">Whether the message is an error.</param>
    void Show(string message, bool isError = false);
}
