using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Series page.
/// </summary>
public class SeriesPage : BaseContentIdPage<SeriesViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SeriesViewModel"/>.</param>
    public SeriesPage(SeriesViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new StackLayout
        {
            Padding = 16,
            Children =
            {
                new Label()
                    .Bind(Label.TextProperty, nameof(ViewModel.Item.Name), source: ViewModel.Item, mode: BindingMode.OneWay)
            }
        };
    }
}
