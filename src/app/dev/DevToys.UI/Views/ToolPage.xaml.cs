using DevToys.Business.Models;
using DevToys.Business.ViewModels;
using DevToys.Core.Tools.ViewItems;
using DevToys.UI.Framework.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DevToys.UI.Views;

/// <summary>
/// Page displaying a tool.
/// </summary>
public sealed partial class ToolPage : Page, IVisualStateListener
{
    public ToolPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the page's view model.
    /// </summary>
    internal ToolPageViewModel ViewModel => (ToolPageViewModel)DataContext;

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        RootUIElementPresenter.Detach();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Guard.IsOfType(e.Parameter, typeof(NavigationParameters<GuiToolViewItem>));
        var parameter = (NavigationParameters<GuiToolViewItem>)e.Parameter;

        DataContext = parameter.MefProvider.Import<ToolPageViewModel>();
        ViewModel.Load(parameter.Parameter);
    }

    public void SetVisualState(string visualStateName)
    {
        Guard.IsNotNullOrWhiteSpace(visualStateName);
        VisualStateManager.GoToState(this, visualStateName, useTransitions: true);
    }
}
