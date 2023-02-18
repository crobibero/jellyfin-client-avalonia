namespace Jellyfin.Mvvm;

/// <summary>
/// Content state types.
/// </summary>
public static class ContentState
{
    /// <summary>
    /// The loading state.
    /// </summary>
    public const string Loading = nameof(Loading);

    /// <summary>
    /// The error state.
    /// </summary>
    public const string Error = nameof(Error);

    /// <summary>
    /// The success state.
    /// </summary>
    public const string Success = nameof(Success);
}
