using Jellyfin.Maui.ContentViews;
using Jellyfin.Maui.DataTemplates;
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
                            Text = Strings.Login_ExistingUsers,
                            Style = BaseStyles.LabelHeader
                        }
                        .Grow(1),
                        new Button { Text = Strings.Login_AddUser }
                            .Bind(Button.CommandProperty, nameof(ViewModel.AddUserCommand)),
                    }
                }
                .Basis(BaseStyles.HeaderBasis),

                // Content
                new ScrollView
                {
                    Content = new ItemFlexLayout
                    {
                        Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
                        ItemTemplate = TemplateHelper.UserSelectTemplate
                    }
                    .Bind(ItemFlexLayout.ItemsSourceProperty, nameof(ViewModel.Users))
                }
                .Grow(1)
            }
        };
    }
}
