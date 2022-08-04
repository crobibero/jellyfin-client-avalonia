using Jellyfin.Maui.Services;
using Microsoft.Extensions.Localization;

namespace Jellyfin.Maui.Extensions;

/// <summary>
/// String localization extension.
/// </summary>
[ContentProperty(nameof(Key))]
public class LocalizeExtension : IMarkupExtension<string>
{
    private readonly IStringLocalizer<Strings> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
    /// </summary>
    public LocalizeExtension()
    {
        _localizer = InternalServiceProvider.GetService<IStringLocalizer<Strings>>();
    }

    /// <summary>
    /// Gets or sets the lookup key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ProvideValue(IServiceProvider serviceProvider)
        => _localizer[Key];

    /// <inheritdoc />
    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}
