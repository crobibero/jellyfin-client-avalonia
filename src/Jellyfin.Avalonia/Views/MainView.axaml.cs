using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.ViewModels;
using Jellyfin.Avalonia.Views.Facades;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The main view.
/// </summary>
public partial class MainView : BaseUserView<MainViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        Current = this;
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Gets the current main view.
    /// </summary>
    public static MainView? Current { get; private set; }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        Current = this;
        base.OnLoaded(e);
    }
}

