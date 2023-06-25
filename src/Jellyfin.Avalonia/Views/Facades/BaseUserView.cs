using Avalonia.Controls;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Views.Facades;

/// <summary>
/// The base user control.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
public class BaseUserView<TViewModel> : UserControl
    where TViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUserView{TViewModel}"/> class.
    /// </summary>
    protected BaseUserView()
    {
        DataContext = ViewModel = ServiceProvider.GetRequiredService<TViewModel>();
    }

    /// <summary>
    /// Gets the view model.
    /// </summary>
    public TViewModel ViewModel { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    protected IServiceProvider ServiceProvider => App.Current.ServiceProvider;

    /// <inheritdoc />
    public override async void EndInit()
    {
        ViewModel.Loading = true;
        await ViewModel.InitializeAsync().ConfigureAwait(true);
        ViewModel.Loading = false;

        base.EndInit();
    }
}
