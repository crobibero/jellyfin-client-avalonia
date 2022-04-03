using Jellyfin.Maui.ContentViews;
using Jellyfin.Maui.DataTemplates;
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
                    .Bind(Label.TextProperty, $"{nameof(ViewModel.Item)}.{nameof(ViewModel.Item.Name)}", mode: BindingMode.OneWay),
                new Label { Text = Strings.NextUp },
                new PosterCardView()
                    .Bind(BindingContextProperty, nameof(ViewModel.NextUpItem), mode: BindingMode.OneTime),
                new Label { Text = Strings.Seasons },
                new CollectionView
                {
                    ItemTemplate = TemplateHelper.PosterCardTemplate,
                    ItemsLayout = LinearItemsLayout.Horizontal,
                    SelectionMode = SelectionMode.Single,
                    ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView
                }
                .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.SeasonsCollection))
            }
        };
    }
}
