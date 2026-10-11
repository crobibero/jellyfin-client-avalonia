namespace Jellyfin.Mvvm.Preferences;

/// <summary>
/// Selectable codec level overrides.
/// </summary>
public static class CodecLevels
{
    /// <summary>
    /// Gets the selectable AVC levels as (display name, level multiplied by 10).
    /// </summary>
    public static IReadOnlyList<(string Name, int Level)> Avc { get; } =
    [
        ("6.2", 62),
        ("6.1", 61),
        ("6.0", 60),
        ("5.2", 52),
        ("5.1", 51),
        ("5.0", 50),
        ("4.2", 42),
        ("4.1", 41),
        ("4.0", 40),
    ];

    /// <summary>
    /// Gets the selectable HEVC levels as (display name, level multiplied by 30).
    /// </summary>
    public static IReadOnlyList<(string Name, int Level)> Hevc { get; } =
    [
        ("6.2", 186),
        ("6.1", 183),
        ("6.0", 180),
        ("5.2", 156),
        ("5.1", 153),
        ("5.0", 150),
        ("4.1", 123),
        ("4.0", 120),
    ];
}
