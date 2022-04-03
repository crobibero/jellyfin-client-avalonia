using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Movie page.
/// </summary>
public class MoviePage : BaseContentIdPage<MovieViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoviePage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="MovieViewModel"/>.</param>
    public MoviePage(MovieViewModel viewModel)
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
            }
        };
    }
}
