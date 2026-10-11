namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class BackdropService : IBackdropService
{
    private readonly IPreferencesService _preferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackdropService"/> class.
    /// </summary>
    /// <param name="preferencesService">The preferences service.</param>
    public BackdropService(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    /// <inheritdoc />
    public event EventHandler? BackdropChanged;

    /// <inheritdoc />
    public string? BackdropUrl { get; private set; }

    /// <inheritdoc />
    public void SetBackdrop(string? url)
    {
        if (!_preferencesService.UserInterface.BackdropEnabled)
        {
            url = null;
        }

        if (string.Equals(url, BackdropUrl, StringComparison.Ordinal))
        {
            return;
        }

        BackdropUrl = url;
        BackdropChanged?.Invoke(this, EventArgs.Empty);
    }
}
