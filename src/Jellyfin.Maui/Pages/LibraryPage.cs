using Jellyfin.Maui.ViewModels;
using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Pages.Facades;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Library page.
/// </summary>
public class LibraryPage : BaseContentIdPage<LibraryViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="LibraryViewModel"/>.</param>
    public LibraryPage(LibraryViewModel viewModel)
        : base(viewModel, "Library")
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
                    .Bind(Label.TextProperty, nameof(ViewModel.Item.Name), BindingMode.OneWay)
            }
        };
    }
}
