using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.MauiBlazor.Components;

namespace DevToys.MauiBlazor.Pages;

public partial class Index : MefComponentBase
{
    [Import]
    internal MainWindowViewModel ViewModel { get; set; } = default!;

    [Import]
    internal GuiToolProvider GuiToolProvider { get; set; } = default!;

    /// <summary>
    /// Indicates whether we're transitioning to another selected menu item.
    /// </summary>
    public bool IsTransitioning { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.SelectedMenuItemChanged += ViewModel_SelectedMenuItemChanged;
        ViewModel.SelectedMenuItem = ViewModel.HeaderAndBodyToolViewItems[0];
    }

    private void ViewModel_SelectedMenuItemChanged(object? sender, EventArgs e)
    {
        // This will force the page content to clear our, disposing the current tool group or tool component before creating a new one, instead
        // of re-using the one currently displayed.
        IsTransitioning = true;
        StateHasChanged();
    }

    private void OnSelectedItemChanged(INotifyPropertyChanged item)
    {
        ViewModel.SelectedMenuItem = item;
    }

    internal void OnSetFavorite()
    {
        // WARNING: This should be done in ToolPageViewModel. Doing it here just for demo.
        if (ViewModel.SelectedMenuItem is GuiToolViewItem guiToolViewItem)
        {
            bool isFavorite = GuiToolProvider.GetToolIsFavorite(guiToolViewItem.ToolInstance);
            GuiToolProvider.SetToolIsFavorite(guiToolViewItem.ToolInstance, !isFavorite);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (IsTransitioning)
        {
            // This will force the page content to re-populate.
            IsTransitioning = false;
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }
}
