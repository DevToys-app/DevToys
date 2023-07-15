using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.Blazor.Components;
using System.Runtime.InteropServices;

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

    private void OnSuggestToolIdeaClick()
    {
        string url = "https://github.com/veler/DevToys/issues/new/choose";
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
