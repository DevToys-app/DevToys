using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.MauiBlazor.Components;

namespace DevToys.MauiBlazor.Pages;

public partial class Index : MefLayoutComponentBase
{
    [Import]
    internal MainWindowViewModel ViewModel { get; set; } = default!;

    [Import]
    internal GuiToolProvider GuiToolProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.SelectedMenuItem = ViewModel.HeaderAndBodyToolViewItems[0];
    }

    internal Task OnSetFavoriteAsync()
    {
        // WARNING: This should be done in ToolPageViewModel. Doing it here just for demo.
        if (ViewModel.SelectedMenuItem is GuiToolViewItem guiToolViewItem)
        {
            bool isFavorite = GuiToolProvider.GetToolIsFavorite(guiToolViewItem.ToolInstance);
            GuiToolProvider.SetToolIsFavorite(guiToolViewItem.ToolInstance, !isFavorite);
        }

        return Task.CompletedTask;
    }
}
