using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DevToys.Api;
using DevToys.Business.ViewModels;
using DevToys.Core;
using DevToys.Core.Models;
using DevToys.Core.Tools;

namespace DevToys.Business.Services;

[Export]
internal sealed class CommandLineLauncherService : ObservableRecipient
{
    internal const string ToolArgument = "tool";

    private readonly GuiToolProvider _guiToolProvider;
    private readonly IMefProvider _mefProvider;

    [ImportingConstructor]
    public CommandLineLauncherService(GuiToolProvider guiToolProvider, IMefProvider mefProvider)
    {
        _mefProvider = mefProvider;
        _guiToolProvider = guiToolProvider;
    }

    internal void HandleCommandLineArguments()
    {
        string toolName = AppHelper.GetCommandLineArgument(ToolArgument);
        if (!string.IsNullOrEmpty(toolName))
        {
            GuiToolInstance? tool = _guiToolProvider.GetToolFromInternalName(toolName);
            if (tool is not null)
            {
                // Make sure MainWindowViewModel is initialized.
                _mefProvider.Import<MainWindowViewModel>();
                Messenger.Send(new ChangeSelectedMenuItemMessage(tool));
            }
        }
    }

    internal void LaunchTool(string toolInternalName)
    {
        GuiToolInstance? tool = _guiToolProvider.GetToolFromInternalName(toolInternalName);
        if (tool is not null)
        {
            string appStartExe = Process.GetCurrentProcess().MainModule!.FileName;
            OSHelper.OpenFileInShell(appStartExe, $"--{ToolArgument}:\"{toolInternalName}\"");
        }
    }
}
