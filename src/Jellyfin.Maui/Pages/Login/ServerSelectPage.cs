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

    private enum Column
    {
        ServerList = 0,
        AddButton = 1
    }

    private enum Row
    {
        Header = 0,
        Content = 1
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new Grid
        {
            ColumnDefinitions = GridRowsColumns.Columns.Define(
                (Column.ServerList, new GridLength(10, GridUnitType.Star)),
                (Column.AddButton, new GridLength(2, GridUnitType.Star))),
            RowDefinitions = GridRowsColumns.Rows.Define(
                (Row.Header, new GridLength(2, GridUnitType.Star)),
                (Row.Content, new GridLength(10, GridUnitType.Star))),
            Children =
            {
                new Label
                    {
                        Text = Strings.Login_ExistingServers,
                        Style = BaseStyles.LabelHeader
                    }
                    .Column(Column.ServerList)
                    .Row(Row.Header),
                new CollectionView
                    {
                        ItemTemplate = TemplateHelper.ServerSelectTemplate,
                        ItemsLayout = LinearItemsLayout.Vertical,
                        SelectionMode = SelectionMode.Single
                    }
                    .Column(Column.ServerList)
                    .Row(Row.Content)
                    .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.Servers)),
                new Button
                    {
                        Text = Strings.Login_AddServer
                    }
                    .Column(Column.AddButton)
                    .Row(Row.Header)
                    .Bind(Button.CommandProperty, nameof(ViewModel.AddServerCommand)),
            }
        };
    }
}
