using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
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
    protected TViewModel ViewModel { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    protected IServiceProvider ServiceProvider => (IServiceProvider)Application.Current!.Resources[typeof(IServiceProvider)]!;

    /// <inheritdoc />
    public override async void EndInit()
    {
        ViewModel.Loading = true;
        await ViewModel.InitializeAsync().ConfigureAwait(true);
        ViewModel.Loading = false;

        base.EndInit();
    }
}
