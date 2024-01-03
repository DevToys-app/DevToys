using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.Blazor.Components;

namespace DevToys.Blazor.Pages.SubPages;

public partial class ToolGroup : MefComponentBase
{
    private GridView<string, GuiToolInstance>? _gridView = default!;

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Pages/SubPages/ToolGroup.razor.js";

    protected string HeroId => Id + "-hero";

    [Import]
    internal ToolGroupPageViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public GroupViewItem? GroupViewItem { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && ViewModel.IsAllToolsGroup)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                Guard.IsNotNull(_gridView);
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("initializeHeroParallax", _gridView.ScrollViewerId, HeroId);
            }
        }
    }

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

    private void OnSuggestToolIdeaClick()
    {
        Shell.OpenFileInShell("https://github.com/veler/DevToys/issues/new/choose");
    }
}
