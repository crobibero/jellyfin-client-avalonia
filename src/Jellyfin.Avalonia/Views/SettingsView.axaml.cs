using Jellyfin.Avalonia.Controls;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The view for <see cref="Jellyfin.Mvvm.ViewModels.SettingsViewModel"/>.
/// </summary>
public partial class SettingsView : PageView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsView"/> class.
    /// </summary>
    public SettingsView()
        => InitializeComponent();
}
