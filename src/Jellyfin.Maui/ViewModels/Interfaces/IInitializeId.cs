namespace Jellyfin.Maui.ViewModels.Interfaces;

/// <summary>
/// View model can be initialized with an id.
/// </summary>
public interface IInitializeId
{
    /// <summary>
    /// Initialize the id for the view model.
    /// </summary>
    /// <param name="id">The current id.</param>
    void Initialize(Guid id);
}
