using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.ViewModels;
using Microsoft.Maui;

namespace Jellyfin.Maui.Pages;

/// <summary>
/// Home page.
/// </summary>
public class HomePage : BaseContentPage<HomeViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/>.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="HomeViewModel"/>.</param>
    public HomePage(HomeViewModel viewModel)
        : base(viewModel, "Home")
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
                new ListView
                {
                    ItemTemplate = new DataTemplate(typeof(BaseItemTemplate))
                }
                .Bind(ListView.ItemsSourceProperty, nameof(ViewModel.ContinueWatching))
            }
        };
    }
}
