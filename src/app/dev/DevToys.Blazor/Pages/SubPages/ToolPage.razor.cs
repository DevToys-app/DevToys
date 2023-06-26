using DevToys.Blazor.Components;
using DevToys.Business.ViewModels;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.Blazor.Pages.SubPages;

public partial class ToolPage : MefComponentBase
{
    [Import]
    internal ToolPageViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public GuiToolViewItem? GuiToolViewItem { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (GuiToolViewItem is not null)
        {
            ViewModel.Load(GuiToolViewItem);
        }
    }
}
