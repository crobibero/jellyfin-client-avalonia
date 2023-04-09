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
        Dispatcher.UIThread.InvokeAsync(action).GetAwaiter().GetResult();
        return true;
    }

    /// <inheritdoc />
    public async Task DispatchAsync(Action action)
        => await Dispatcher.UIThread.InvokeAsync(action).ConfigureAwait(false);

    /// <inheritdoc />
    public void EnableCollectionSynchronization(IEnumerable collection, object? context, Action<IEnumerable, object, Action, bool> callback)
    {
        // TODO?
    }
}
