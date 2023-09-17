using AvaloniaInside.Shell;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Avalonia.Views.Facades;

/// <summary>
/// The base user control.
/// </summary>
public class BaseUserView : Page
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
    public override async Task AppearAsync(CancellationToken cancellationToken)
    {
        await BaseViewModel.InitializeAsync().ConfigureAwait(true);
        await base.AppearAsync(cancellationToken).ConfigureAwait(true);
    }
}
