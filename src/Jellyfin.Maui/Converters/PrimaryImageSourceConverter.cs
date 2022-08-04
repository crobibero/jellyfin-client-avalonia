namespace Jellyfin.Maui.Converters;

/// <summary>
/// Gets the poster url from the base item.
/// </summary>
public class PrimaryImageSourceConverter : BaseImageSourceConverter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaryImageSourceConverter"/> class.
    /// </summary>
    public PrimaryImageSourceConverter()
      : base(ImageType.Primary)
    {
    }
}
