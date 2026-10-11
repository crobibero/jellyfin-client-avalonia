using System.Diagnostics.CodeAnalysis;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// View model first navigation service.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Raised after the current view model changed.
    /// </summary>
    event EventHandler? Navigated;

    /// <summary>
    /// Gets the current view model.
    /// </summary>
    BaseViewModel? Current { get; }

    /// <summary>
    /// Gets a value indicating whether there is a page to go back to.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Navigate to a view model.
    /// </summary>
    /// <param name="parameter">The navigation parameter.</param>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NavigateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(object? parameter = null)
        where TViewModel : BaseViewModel;

    /// <summary>
    /// Navigate to a view model and clear the back stack.
    /// </summary>
    /// <param name="parameter">The navigation parameter.</param>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task NavigateRootAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(object? parameter = null)
        where TViewModel : BaseViewModel;

    /// <summary>
    /// Go back to the previous view model.
    /// </summary>
    /// <returns><c>true</c> when navigation happened.</returns>
    Task<bool> GoBackAsync();
}
