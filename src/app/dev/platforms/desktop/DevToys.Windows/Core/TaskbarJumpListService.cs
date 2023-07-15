using System.Windows.Shell;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DevToys.Api;
using DevToys.Business.ViewModels;
using DevToys.Core.Models;
using DevToys.Core.Tools;
using DevToys.Windows.Helpers;
using DevToys.Windows.Strings.TaskBar;

namespace DevToys.Windows.Core;

[Export(typeof(TaskbarJumpListService))]
internal class TaskbarJumpListService : ObservableRecipient
{
    private const string ToolArgument = "--tool:";

    private readonly GuiToolProvider _guiToolProvider;
    private readonly IMefProvider _mefProvider;

    [ImportingConstructor]
    public TaskbarJumpListService(GuiToolProvider guiToolProvider, IMefProvider mefProvider)
    {
        _mefProvider = mefProvider;
        _guiToolProvider = guiToolProvider;
        _guiToolProvider.FavoriteToolsChanged += GuiToolProvider_FavoriteToolsChanged;

        RefreshJumpListAsync().Forget();

        HandleOpenWithJumpList();
    }

    private void GuiToolProvider_FavoriteToolsChanged(object? sender, EventArgs e)
    {
        RefreshJumpListAsync().Forget();
    }

    private void HandleOpenWithJumpList()
    {
        if (Environment.CommandLine.Contains(ToolArgument))
        {
            string toolName = Environment.CommandLine.Substring(Environment.CommandLine.IndexOf(ToolArgument) + ToolArgument.Length);
            GuiToolInstance? tool = _guiToolProvider.GetToolFromInternalName(toolName);
            if (tool is not null)
            {
                // Make sure MainWindowViewModel is initialized.
                _mefProvider.Import<MainWindowViewModel>();
                Messenger.Send(new ChangeSelectedMenuItemMessage(tool));
            }
        }
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
            Arguments = $"{ToolArgument}{tool.InternalComponentName}"
        };

        jumpList.JumpItems.Add(jumpTask);
    }
}
