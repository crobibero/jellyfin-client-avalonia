using System;

namespace Jellyfin.Maui.ViewModels
{
    /// <summary>
    /// Provides initialization interface for id.
    /// </summary>
    public interface IInitializeId
    {
        /// <summary>
        /// Initialize the id for the view model.
        /// </summary>
        /// <param name="id">The current id.</param>
        void Initialize(Guid id);
    }
}
