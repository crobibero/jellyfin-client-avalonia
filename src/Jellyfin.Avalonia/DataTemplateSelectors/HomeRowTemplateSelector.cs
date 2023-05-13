using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Jellyfin.Mvvm.Models;

namespace Jellyfin.Avalonia.DataTemplateSelectors;

/// <summary>
/// Home row template selector.
/// </summary>
public class HomeRowTemplateSelector : IDataTemplate
{
    /// <summary>
    /// Gets or sets the library home row template.
    /// </summary>
    public DataTemplate? LibraryHomeRowTemplate { get; set; }

    /// <inheritdoc />
    public Control? Build(object? param)
    {
        if (param is HomeRowModel homeRowModel)
        {
            return LibraryHomeRowTemplate!.Build(homeRowModel);
        }

        return null;
    }

    /// <inheritdoc />
    public bool Match(object? data) => true;
}
