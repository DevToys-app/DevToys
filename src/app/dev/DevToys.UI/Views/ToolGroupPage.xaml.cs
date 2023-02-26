using DevToys.UI.Models;
using DevToys.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DevToys.UI.Views;

/// <summary>
/// Page displaying a group of tools.
/// </summary>
public sealed partial class ToolGroupPage : Page
{
    private const int GridViewItemMaxWidth = 430;

    public ToolGroupPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the page's view model.
    /// </summary>
    internal ToolGroupPageViewModel ViewModel => (ToolGroupPageViewModel)DataContext;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Guard.IsOfType(e.Parameter, typeof(NavigationParameters<string>));
        var parameter = (NavigationParameters<string>)e.Parameter;

        DataContext = parameter.MefProvider.Import<ToolGroupPageViewModel>();
        ViewModel.Load(groupName: parameter.Parameter);
    }

    private void GridView_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        var gridViewItemMargin = (Thickness)this.Resources["GridViewItemMargin"];

        // Calculating the number of columns based on the width of the page
        double columns = Math.Ceiling(ActualWidth / (GridViewItemMaxWidth + gridViewItemMargin.Left + gridViewItemMargin.Right));
        ((ItemsWrapGrid)GridView.ItemsPanelRoot).ItemWidth = args.NewSize.Width / columns;
    }
}
