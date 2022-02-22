using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The server select page.
/// </summary>
public class ServerSelectPage : BaseContentPage<ServerSelectViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerSelectPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="ServerSelectViewModel"/>.</param>
    public ServerSelectPage(ServerSelectViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Button { Text = Strings.Login_AddServer }
                    .Bind(Button.CommandProperty, nameof(ViewModel.AddServerCommand)),
                new Label { Text = Strings.Login_ExistingServers },
                new CollectionView
                    {
                        ItemTemplate = TemplateHelper.ServerSelectTemplate,
                        ItemsLayout = LinearItemsLayout.Vertical,
                        SelectionMode = SelectionMode.Single
                    }
                    .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.Servers))
                    .Bind(SelectableItemsView.SelectedItemProperty, nameof(ViewModel.SelectedServer))
                    .Bind(SelectableItemsView.SelectionChangedCommandProperty, nameof(ViewModel.SelectServerCommand))
            }
        };
    }
}
