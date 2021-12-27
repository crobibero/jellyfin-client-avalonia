using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels;
using Jellyfin.Maui.ViewModels.Login;
using Microsoft.Maui.Layouts;

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
        Content = new StackLayout
        {
            new Label { Text = "Stored Servers" },
            new Button { Text = "Add Server" }
                .Bind(Button.CommandProperty, nameof(ViewModel.AddServerCommand)),
            new ScrollView
            {
                Padding = 16,
                Content = new FlexLayout
                    {
                        Wrap = FlexWrap.Wrap,
                        Direction = FlexDirection.Row
                    }
                    .ItemTemplate(new ServerSelectTemplate())
                    .Bind(BindableLayout.ItemsSourceProperty, nameof(ViewModel.Servers))
            },
        };
    }
}
