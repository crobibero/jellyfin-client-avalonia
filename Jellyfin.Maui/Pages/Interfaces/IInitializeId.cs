using System;

namespace Jellyfin.Maui.Pages
{
    public interface IInitializeId
    {
        /// <summary>
        /// Initialize the id for the view.
        /// </summary>
        /// <param name="id">The current id.</param>
        void Initialize(Guid id);
    }
}
