using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Jellyfin.Avalonia.ViewModels;
using Jellyfin.Avalonia.Views;
using Jellyfin.Mvvm.ViewModels.Facades;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Avalonia.Services;

/// <summary>
/// View locator.
/// </summary>
public class ViewLocator : IDataTemplate
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewLocator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Instance of the <see cref="IServiceProvider"/> interface.</param>
    public ViewLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Control Build(object? param)
    {
        ArgumentNullException.ThrowIfNull(param);

        var paramType = param.GetType();
        Type? type;

        if (paramType == typeof(MainWindowViewModel))
        {
            type = typeof(MainWindow);
        }
        else if (paramType == typeof(ContentNavigationViewModel))
        {
            type = typeof(ContentNavigationView);
        }
        else if (paramType == typeof(LoadingViewModel))
        {
            type = typeof(LoadingView);
        }
        else
        {
            var name = paramType.FullName!
                .Replace("Jellyfin.Mvvm", "Jellyfin.Avalonia", StringComparison.Ordinal)
                .Replace("ViewModel", "View", StringComparison.Ordinal);
            type = Type.GetType(name);

            if (type is null)
            {
                return new TextBlock { Text = $"Not Found: {name}" };
            }
        }

        using var scope = _serviceProvider.CreateScope();
        return (Control)scope.ServiceProvider.GetRequiredService(type);
    }

    /// <inheritdoc />
    public bool Match(object? data)
        => data is BaseViewModel;
}
