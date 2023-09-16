#pragma warning disable SA1649 // Using generic type filename.

using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Views.Facades;

/// <summary>
/// The base user control.
/// </summary>
/// <typeparam name="TViewModel">The type of view model.</typeparam>
public class BaseUserView<TViewModel> : BaseUserView
    where TViewModel : BaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUserView{TViewModel}"/> class.
    /// </summary>
    protected BaseUserView()
    : this(ServiceProvider.GetRequiredService<TViewModel>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUserView{TViewModel}"/> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    private BaseUserView(TViewModel viewModel)
        : base(viewModel)
    {
        DataContext = ViewModel = viewModel;
    }

    /// <summary>
    /// Gets the view model.
    /// </summary>
    public TViewModel ViewModel { get; }
}
