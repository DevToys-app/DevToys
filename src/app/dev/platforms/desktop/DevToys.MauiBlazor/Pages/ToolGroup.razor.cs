using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.MauiBlazor.Components;

namespace DevToys.MauiBlazor.Pages;

public partial class ToolGroup : MefLayoutComponentBase
{
    [Import]
    internal ToolGroupPageViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public GroupViewItem? GroupViewItem { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (GroupViewItem is not null)
        {
            ViewModel.Load(GroupViewItem);
        }
    }

    internal void OnToolSelected(object item)
    {
        Guard.IsOfType<GuiToolInstance>(item);
        ViewModel.ToolSelectedCommand.Execute((GuiToolInstance)item);
    }
}
