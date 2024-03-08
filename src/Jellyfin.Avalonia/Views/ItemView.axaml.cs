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
        => InitializeComponent();

    /// <inheritdoc />
    public override async Task ArgumentAsync(object args, CancellationToken cancellationToken)
    {
        if (args is Guid itemId)
        {
            await ViewModel.InitializeAsync(itemId).ConfigureAwait(true);
        }

        await base.ArgumentAsync(args, cancellationToken).ConfigureAwait(true);
    }
}
