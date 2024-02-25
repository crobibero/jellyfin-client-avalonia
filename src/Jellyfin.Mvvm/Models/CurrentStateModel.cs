namespace Jellyfin.Mvvm.Models;

/// <summary>
/// The state model.
/// </summary>
public class CurrentStateModel
{
    /// <summary>
    /// Gets or sets the current token.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the current host.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the current user.
    /// </summary>
    public UserDto? UserDto { get; set; }

    /// <summary>
    /// Gets or sets the server state.
    /// </summary>
    public ServerStateModel? ServerState { get; set; }

    /// <summary>
    /// Gets or sets the user state.
    /// </summary>
    public UserStateModel? UserState { get; set; }

    /// <summary>
    /// Gets or sets static host.
    /// </summary>
    public static string? StaticHost { get; set; }
}
