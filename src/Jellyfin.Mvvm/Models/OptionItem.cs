namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A selectable option.
/// </summary>
/// <param name="Label">The display label.</param>
/// <param name="Value">The value.</param>
/// <typeparam name="T">The value type.</typeparam>
public sealed record OptionItem<T>(string Label, T Value)
{
    /// <inheritdoc />
    public override string ToString() => Label;
}
