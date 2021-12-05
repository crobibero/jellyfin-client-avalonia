using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Season page.
/// </summary>
public class SeasonPage : BaseContentIdPage<SeasonViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SeasonViewModel"/>.</param>
    public SeasonPage(SeasonViewModel viewModel)
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
