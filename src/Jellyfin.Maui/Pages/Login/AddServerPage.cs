using CommunityToolkit.Maui.Markup;
using Jellyfin.Maui.Pages.Facades;
using Jellyfin.Maui.ViewModels.Login;

namespace Jellyfin.Maui.Pages.Login;

/// <summary>
/// The add server page.
/// </summary>
public class AddServerPage : BaseContentPage<AddServerViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddServerPage"/> class.
    /// </summary>
    /// <param name="viewModel">Instance of the <see cref="AddServerViewModel"/>.</param>
    public AddServerPage(AddServerViewModel viewModel)
        : base(viewModel)
    {
    }

    /// <inheritdoc />
    protected override void InitializeLayout()
    {
        Content = new StackLayout
        {
            Children =
            {
                new Label { Text = Strings.Login_ServerUrl },
                new Entry()
                    .FillHorizontal()
                    .Bind(Entry.TextProperty, nameof(ViewModel.ServerUrl)),
                new Button { Text = Strings.Add }
                    .FillHorizontal()
                    .Bind(Button.CommandProperty, nameof(ViewModel.AddServerCommand))
            }
        };
    }
}
