namespace Jellyfin.Mvvm.Models;

/// <summary>
/// Display preferences mapping model.
/// </summary>
public class DisplayPreferencesModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayPreferencesModel"/> class.
    /// </summary>
    /// <param name="homeSections">The list of home sections.</param>
    public DisplayPreferencesModel(IReadOnlyList<HomeSection> homeSections)
    {
        HomeSections = homeSections;
    }

    /// <summary>
    /// Home section type.
    /// </summary>
    public enum HomeSection
    {
        /// <summary>
        /// No home section.
        /// </summary>
        None = 0,

        /// <summary>
        /// Library tiles home section.
        /// </summary>
        LibraryTiles = 1,

        /// <summary>
        /// Resume home section.
        /// </summary>
        Resume = 2,

        /// <summary>
        /// Next up home section.
        /// </summary>
        NextUp = 3,

        /// <summary>
        /// Latest media home section.
        /// </summary>
        LatestMedia = 4
    }

    /// <summary>
    /// Gets the list of home sections.
    /// </summary>
    public IReadOnlyList<HomeSection> HomeSections { get; }
}
