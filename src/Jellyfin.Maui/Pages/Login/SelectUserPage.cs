using Jellyfin.Maui.DataTemplates;
using Jellyfin.Maui.Models;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The select user page.
/// </summary>
public class SelectUserPage : BaseContentPage<SelectUserViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectUserPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="SelectUserViewModel"/>.</param>
    public SelectUserPage(SelectUserViewModel viewModel)
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
                new Button { Text = Strings.Login_AddUser }
                    .Bind(Button.CommandProperty, nameof(ViewModel.AddUserCommand)),
                new Label { Text = Strings.Login_ExistingUsers },
                new CollectionView
                    {
                        ItemTemplate = TemplateHelper.UserSelectTemplate,
                        ItemsLayout = LinearItemsLayout.Vertical,
                        SelectionMode = SelectionMode.Single
                    }
                    .Bind(ItemsView.ItemsSourceProperty, nameof(ViewModel.Users))
            }
        };
    }
}
