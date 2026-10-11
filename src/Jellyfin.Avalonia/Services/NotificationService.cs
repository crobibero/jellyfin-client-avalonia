using System.Collections.ObjectModel;
using Avalonia.Threading;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Shows transient toast messages.
/// </summary>
public class NotificationService : INotificationService
{
    private static readonly TimeSpan _duration = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Gets the visible toasts.
    /// </summary>
    public ObservableCollection<ToastModel> Toasts { get; } = new();

    /// <inheritdoc />
    public void Show(string message, bool isError = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var toast = new ToastModel(message, isError);
            Toasts.Add(toast);
            DispatcherTimer.RunOnce(() => Toasts.Remove(toast), _duration);
        });
    }
}

/// <summary>
/// A toast message.
/// </summary>
/// <param name="Message">The message.</param>
/// <param name="IsError">Whether the message is an error.</param>
public sealed record ToastModel(string Message, bool IsError);
