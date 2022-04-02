using Jellyfin.Maui.ContentViews;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Library page.
/// </summary>
public class LibraryPage : BaseContentIdPage<LibraryViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="LibraryViewModel"/>.</param>
    public LibraryPage(LibraryViewModel viewModel)
        : base(viewModel, Strings.Library)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new FlexLayout
        {
            Direction = Microsoft.Maui.Layouts.FlexDirection.Column,
            Children =
            {
                /*
                 * TODO how to make this work?
                new Label()
                    .Bind(Label.TextProperty, nameof(ViewModel.Item.Name), source: ViewModel.Item, mode: BindingMode.OneWay),
                */
                // Header
                new Label()
                    .Bind(Label.TextProperty, "Item.Name", mode: BindingMode.OneWay)
                    .Basis(BaseStyles.HeaderBasis),

                // Content
                new ScrollView
                {
                    Content = new ItemFlexLayout
                    {
                        Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
                        ItemTemplate = TemplateHelper.PosterCardTemplate,
                    }
                    .Bind(ItemFlexLayout.ItemsSourceProperty, mode: BindingMode.OneWay, path: nameof(LibraryViewModel.LibraryItemsCollection))
                }
                .Grow(1)
            }
        };
    }
}
