using Jellyfin.Maui.ContentViews;
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
        Content = new FlexLayout
        {
            Direction = Microsoft.Maui.Layouts.FlexDirection.Column,
            Children =
            {
                // Header
                new FlexLayout
                {
                    Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
                    Children =
                    {
                        new Label
                        {
                            Text = Strings.Login_ExistingServers,
                            Style = BaseStyles.LabelHeader
                        }
                        .Grow(1),
                        new Button { Text = Strings.Login_AddServer }
                            .Bind(Button.CommandProperty, nameof(ViewModel.AddServerCommand)),
                    }
                }
                .Basis(BaseStyles.HeaderBasis),

                // Content
                new ScrollView
                {
                    Content = new ItemFlexLayout
                    {
                        Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
                        ItemTemplate = TemplateHelper.ServerSelectTemplate
                    }
                    .Bind(ItemFlexLayout.ItemsSourceProperty, nameof(ViewModel.Servers))
                }
                .Grow(1)
            }
        };
    }
}
