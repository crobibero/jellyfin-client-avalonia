using Jellyfin.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace Jellyfin.Maui.Pages
{
    abstract class BaseContentPage<T> 
        : ContentPage where T : BaseViewModel
    {
        protected BaseContentPage(T viewModel)
            :this(viewModel, string.Empty)
        {
        }

        protected BaseContentPage(T viewModel, string pageTitle)
        {
            BindingContext = ViewModel = viewModel;
            Title = pageTitle;

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            InitializeLayout();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected T ViewModel { get; }

        protected abstract void InitializeLayout();
    }
}
