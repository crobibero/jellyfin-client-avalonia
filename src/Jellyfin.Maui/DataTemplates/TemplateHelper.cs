namespace Jellyfin.Maui.DataTemplates;

/// <summary>
/// Static DataTemplate container.
/// </summary>
public static class TemplateHelper
{
    /// <summary>
    /// Gets the <see cref="HomeRowTemplateSelector"/>.
    /// </summary>
    public static readonly HomeRowTemplateSelector HomeRowTemplateSelector = new();

    /// <summary>
    /// Gets the <see cref="CardTemplateSelector"/>.
    /// </summary>
    public static readonly CardTemplateSelector CardTemplateSelector = new();

    /// <summary>
    /// Gets the <see cref="DataTemplates.LibraryHomeRowTemplate"/>.
    /// </summary>
    public static readonly LibraryHomeRowTemplate LibraryHomeRowTemplate = new();

    /// <summary>
    /// Gets the <see cref="PosterCardTemplate"/>.
    /// </summary>
    public static readonly PosterCardTemplate PosterCardTemplate = new();

    /// <summary>
    /// Gets the <see cref="ServerSelectTemplate"/>.
    /// </summary>
    public static readonly ServerSelectTemplate ServerSelectTemplate = new();

    /// <summary>
    /// Gets the <see cref="UserSelectTemplate"/>.
    /// </summary>
    public static readonly UserSelectTemplate UserSelectTemplate = new();
}
