using Avalonia.Markup.Xaml;

namespace Jellyfin.Avalonia.MarkupExtensions;

/// <summary>
/// Int32 markup extension.
/// </summary>
public class Int32Extension : MarkupExtension
{
    private readonly int _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Int32Extension"/> class.
    /// </summary>
    /// <param name="value">The int value.</param>
    public Int32Extension(int value)
    {
        _value = value;
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
        => _value;
}
