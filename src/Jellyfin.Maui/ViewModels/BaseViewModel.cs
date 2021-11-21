using CommunityToolkit.Mvvm.ComponentModel;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Base view model.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    /// <summary>
    /// Initialize the view model.
    /// </summary>
    /// <returns>A Task.</returns>
    public abstract ValueTask InitializeAsync();
}
