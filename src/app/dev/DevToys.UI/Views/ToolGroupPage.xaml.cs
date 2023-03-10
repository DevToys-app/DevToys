using DevToys.Business.Models;
using DevToys.Business.ViewModels;
using DevToys.Core.Tools.ViewItems;
using DevToys.UI.Framework.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DevToys.UI.Views;

/// <summary>
/// Page displaying a group of tools.
/// </summary>
public sealed partial class ToolGroupPage : Page, IVisualStateListener
{
    private const int GridViewItemMinWidth = 330;

    public ToolGroupPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the page's view model.
    /// </summary>
    internal ToolGroupPageViewModel ViewModel => (ToolGroupPageViewModel)DataContext;

    public void SetVisualState(string visualStateName)
    {
        Guard.IsNotNullOrWhiteSpace(visualStateName);
        Guard.IsNotEqualTo(visualStateName, MainWindow.CompactOverlayStateName);
        VisualStateManager.GoToState(this, visualStateName, useTransitions: true);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Guard.IsOfType(e.Parameter, typeof(NavigationParameters<GroupViewItem>));
        var parameter = (NavigationParameters<GroupViewItem>)e.Parameter;

        DataContext = parameter.MefProvider.Import<ToolGroupPageViewModel>();
        ViewModel.Load(parameter.Parameter);
    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.ToolSelectedCommand.Execute(e.ClickedItem);
    }

    private void GridView_SizeChanged(object sender, SizeChangedEventArgs args)
    {
        var gridViewItemMargin = (Thickness)this.Resources["GridViewItemMargin"];

        // Calculating the number of columns based on the width of the page
        double gridViewItemHorizontalSpace = args.NewSize.Width - GridView.Padding.Left - GridView.Padding.Right;
        double adjustedGridViewItemMaxWidth = GridViewItemMinWidth + gridViewItemMargin.Left + gridViewItemMargin.Right;
        double columns = Math.Floor(gridViewItemHorizontalSpace / adjustedGridViewItemMaxWidth);

        // Calculating the new width of the grid view item.
        double newItemWidth = Math.Max(GridViewItemMinWidth, gridViewItemHorizontalSpace / Math.Max(1, columns));
        ((ItemsWrapGrid)GridView.ItemsPanelRoot).ItemWidth = newItemWidth;
    }
}
