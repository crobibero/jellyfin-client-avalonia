using System.Collections;
using Avalonia.Threading;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// Implementation of the <see cref="IApplicationService"/>.
/// </summary>
public class ApplicationService : IApplicationService
{
    /// <inheritdoc />
    public bool Dispatch(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
        return true;
    }

    /// <inheritdoc />
    public async Task DispatchAsync(Action action)
        => await Dispatcher.UIThread.InvokeAsync(action);

    /// <inheritdoc />
    public void EnableCollectionSynchronization(IEnumerable collection, object? context, Action<IEnumerable, object, Action, bool> callback)
    {
        // TODO?
    }
}
