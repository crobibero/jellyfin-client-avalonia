namespace Jellyfin.Maui.ContentViews;

/// <summary>
/// The library card view.
/// </summary>
public class LibraryCardView : BaseCardView
{
    private static readonly (Row, GridLength)[] _rowDefinitions = new (Row, GridLength)[]
    {
        (Row.Image, 300),
        (Row.Title, 50)
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryCardView"/> class.
    /// </summary>
    public LibraryCardView()
        : base(
              BaseStyles.LibraryCard,
              null,
              null,
              null,
              ImageType.Primary,
              _rowDefinitions)
    {
    }
}
