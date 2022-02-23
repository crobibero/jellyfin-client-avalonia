namespace Jellyfin.Maui.ContentViews.Facades;

/// <summary>
/// The base content view.
/// </summary>
/// <typeparam name="T">The type of data context.</typeparam>
public class BaseContentView<T> : ContentView
{
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public T? Context { get; private set; }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        Context = (T)BindingContext;
        base.OnBindingContextChanged();
    }
}
