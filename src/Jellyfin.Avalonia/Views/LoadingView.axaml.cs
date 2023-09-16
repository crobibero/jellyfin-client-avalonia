using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.ViewModels;
using Jellyfin.Avalonia.Views.Facades;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// The loading view.
/// </summary>
public partial class LoadingView : BaseUserView<LoadingViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingView"/> class.
    /// </summary>
    public LoadingView()
        => AvaloniaXamlLoader.Load(this);
}
