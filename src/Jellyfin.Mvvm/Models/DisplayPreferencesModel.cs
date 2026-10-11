using System.Globalization;

namespace Jellyfin.Mvvm.Models;

/// <summary>
/// Display preferences mapping model.
/// </summary>
public class DisplayPreferencesModel
{
    private static readonly HomeSection[] _defaultSections =
    [
        HomeSection.LibraryTiles,
        HomeSection.Resume,
        HomeSection.ResumeAudio,
        HomeSection.NextUp,
        HomeSection.LatestMedia,
    ];

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
        LatestMedia = 4,

        /// <summary>
        /// Resume audio home section.
        /// </summary>
        ResumeAudio = 5,

        /// <summary>
        /// Resume book home section.
        /// </summary>
        ResumeBook = 6
    }

    /// <summary>
    /// Gets the default display preferences.
    /// </summary>
    public static DisplayPreferencesModel Default { get; } = new(_defaultSections);

    /// <summary>
    /// Gets the list of home sections.
    /// </summary>
    public IReadOnlyList<HomeSection> HomeSections { get; }

    /// <summary>
    /// Create the display preferences from the web client's custom preferences.
    /// </summary>
    /// <param name="customPreferences">The custom preferences.</param>
    /// <returns>The display preferences.</returns>
    public static DisplayPreferencesModel FromCustomPreferences(IDictionary<string, object>? customPreferences)
    {
        if (customPreferences is null)
        {
            return Default;
        }

        var configured = new SortedDictionary<int, string>();
        foreach (var (key, value) in customPreferences)
        {
            if (key.StartsWith("homesection", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(key.AsSpan("homesection".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
            {
                configured[index] = value?.ToString() ?? string.Empty;
            }
        }

        if (configured.Count == 0)
        {
            return Default;
        }

        var sections = new List<HomeSection>();
        for (var i = 0; i < _defaultSections.Length || configured.ContainsKey(i); i++)
        {
            var section = configured.TryGetValue(i, out var value)
                ? Parse(value)
                : (i < _defaultSections.Length ? _defaultSections[i] : HomeSection.None);

            if (section is not HomeSection.None && !sections.Contains(section))
            {
                sections.Add(section);
            }
        }

        return new DisplayPreferencesModel(sections);
    }

    private static HomeSection Parse(string value) => value.ToLowerInvariant() switch
    {
        "smalllibrarytiles" or "librarybuttons" => HomeSection.LibraryTiles,
        "resume" => HomeSection.Resume,
        "resumeaudio" => HomeSection.ResumeAudio,
        "resumebook" => HomeSection.ResumeBook,
        "nextup" => HomeSection.NextUp,
        "latestmedia" => HomeSection.LatestMedia,
        _ => HomeSection.None
    };
}
