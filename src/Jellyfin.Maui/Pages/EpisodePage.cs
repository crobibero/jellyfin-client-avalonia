using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Episode page.
/// </summary>
public class EpisodePage : BaseContentIdPage<EpisodeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodePage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="EpisodeViewModel"/>.</param>
    public EpisodePage(EpisodeViewModel viewModel)
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
