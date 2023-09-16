using Avalonia.Markup.Xaml;
using Jellyfin.Avalonia.Views.Facades;
using Jellyfin.Mvvm.ViewModels;

namespace Jellyfin.Avalonia.Views;

/// <summary>
/// Basic item view.
/// </summary>
public partial class ItemView : BaseUserView<ItemViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ItemView"/> class.
    /// </summary>
    public ItemView()
        => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override Task ArgumentAsync(object args, CancellationToken cancellationToken)
    {
        if (args is Guid itemId)
        {
            ViewModel.InitializeItemId(itemId);
        }

        return base.ArgumentAsync(args, cancellationToken);
    }
}
