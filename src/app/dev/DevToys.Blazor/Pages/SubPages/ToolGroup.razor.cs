using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.Blazor.Components;

namespace DevToys.Blazor.Pages.SubPages;

public partial class ToolGroup : MefComponentBase
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

    private void OnToolSelected(object item)
    {
        Guard.IsOfType<GuiToolInstance>(item);
        ViewModel.ToolSelectedCommand.Execute((GuiToolInstance)item);
    }

    private void OnOpenInNewWindow(GuiToolInstance item)
    {
        // TODO
    }
}
