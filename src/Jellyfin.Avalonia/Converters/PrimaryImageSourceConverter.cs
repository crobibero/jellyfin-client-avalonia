using Jellyfin.Sdk;

namespace Jellyfin.Avalonia.Converters;

/// <summary>
/// Gets the primary image url from the item.
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
