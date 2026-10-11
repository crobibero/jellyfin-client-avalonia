using System.Collections.Specialized;
using Avalonia.Threading;
using Jellyfin.Avalonia.Controls;
using Jellyfin.Mvvm.ViewModels.Login;

namespace Jellyfin.Avalonia.Views.Login;

/// <summary>
/// The view for <see cref="SelectUserViewModel"/>.
/// </summary>
public partial class SelectUserView : PageView
{
    private SelectUserViewModel? _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserView"/> class.
    /// </summary>
    public SelectUserView()
        => InitializeComponent();

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.Users.CollectionChanged -= OnUsersChanged;
        }

        _viewModel = DataContext as SelectUserViewModel;
        if (_viewModel is not null)
        {
            _viewModel.Users.CollectionChanged += OnUsersChanged;
        }

        base.OnDataContextChanged(e);
    }

    private void OnUsersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Users load from the server, so the buttons below them can take focus first.
        // Wait for the user cards to be created, then focus the first one.
        Dispatcher.UIThread.Post(FocusInitialIfUntouched, DispatcherPriority.Loaded);
    }
}
