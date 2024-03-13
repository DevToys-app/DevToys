using System.Windows.Shell;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Api;
using DevToys.Business.Services;
using DevToys.Core.Tools;
using DevToys.Windows.Helpers;
using DevToys.Windows.Strings.TaskBar;

namespace DevToys.Windows.Core;

[Export(typeof(TaskbarJumpListService))]
internal class TaskbarJumpListService : ObservableRecipient
{
    private readonly GuiToolProvider _guiToolProvider;

    [ImportingConstructor]
    public TaskbarJumpListService(GuiToolProvider guiToolProvider)
    {
        _guiToolProvider = guiToolProvider;
        _guiToolProvider.FavoriteToolsChanged += GuiToolProvider_FavoriteToolsChanged;

        RefreshJumpListAsync().Forget();
    }

    private void GuiToolProvider_FavoriteToolsChanged(object? sender, EventArgs e)
    {
        RefreshJumpListAsync().Forget();
    }

    private Task RefreshJumpListAsync()
    {
        return ThreadHelper.RunOnUIThreadAsync(() =>
        {
            var jumpList = JumpList.GetJumpList(System.Windows.Application.Current);
            if (jumpList is null)
            {
                jumpList = new JumpList();
                JumpList.SetJumpList(System.Windows.Application.Current, jumpList);
            }

            ClearJumpList(jumpList);

            for (int i = 0; i < _guiToolProvider.AllTools.Count; i++)
            {
                GuiToolInstance tool = _guiToolProvider.AllTools[i];
                if (_guiToolProvider.GetToolIsFavorite(tool))
                {
                    AddToolToJumpList(jumpList, tool, TaskBar.FavoriteTools);
                }
            }

            jumpList.Apply();
        });
    }

    private static void ClearJumpList(JumpList jumpList)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        jumpList.JumpItems.Clear();
    }

    private static void AddToolToJumpList(JumpList jumpList, GuiToolInstance tool, string category)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var jumpTask = new JumpTask
        {
            Title = tool.LongOrShortDisplayTitle,
            Description = tool.Description,
            CustomCategory = category,
            Arguments = $"--{CommandLineLauncherService.ToolArgument}:\"{tool.InternalComponentName}\""
        };

        jumpList.JumpItems.Add(jumpTask);
    }
}
