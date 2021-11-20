using Jellyfin.Maui.ViewModels.Interfaces;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// ViewModel that has an ID parameter.
/// </summary>
public class BaseIdViewModel : BaseViewModel, IInitializeId
{
    private Guid _id;

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// Initialize the view model's id.
    /// </summary>
    /// <param name="id">The id.</param>
    public void Initialize(Guid id) => Id = id;
}
