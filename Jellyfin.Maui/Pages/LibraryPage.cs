using Jellyfin.Maui.ViewModels;
using CommunityToolkit.Maui.Markup;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Library page.
/// </summary>
public class LibraryPage : BaseContentPage<LibraryViewModel>, IInitializeId
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
    public void Initialize(Guid id)
    {
        ViewModel.Initialize(id);
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
                    .Bind(Label.TextProperty, nameof(ViewModel.Id), BindingMode.OneWay)
            }
        };
    }
}
