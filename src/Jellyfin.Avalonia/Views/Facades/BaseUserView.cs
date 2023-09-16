using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaInside.Shell;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Avalonia.Views.Facades;

/// <summary>
/// The base user control.
/// </summary>
public class BaseUserView : UserControl, INavigationLifecycle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUserView"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="BaseViewModel"/>.</param>
    protected BaseUserView(BaseViewModel viewModel)
    {
        BaseViewModel = viewModel;
    }

    /// <summary>
    /// Gets the base view model.
    /// </summary>
    public BaseViewModel BaseViewModel { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    protected static IServiceProvider ServiceProvider => App.Current.ServiceProvider;

    /// <inheritdoc />
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        BaseViewModel.Loading = true;
        await BaseViewModel.InitializeAsync().ConfigureAwait(true);
        BaseViewModel.Loading = false;
        base.OnLoaded(e);
    }

    /* TODO inherit from Page when Shell library updates */

    /// <inheritdoc />
    public virtual Task InitialiseAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task AppearAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task DisappearAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task ArgumentAsync(object args, CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task TerminateAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
