using CommunityToolkit.Mvvm.Messaging.Messages;
using DevToys.Core.Tools;

namespace DevToys.Business.Models;

internal sealed class ChangeSelectedMenuItemMessage : ValueChangedMessage<GuiToolInstance>
{
    internal ChangeSelectedMenuItemMessage(GuiToolInstance tool)
        : base(tool)
    {
        Guard.IsNotNull(tool);
        SmartDetectionInfo = null;
    }

    internal ChangeSelectedMenuItemMessage(SmartDetectedTool smartDetectionInfo)
        : base(smartDetectionInfo.ToolInstance)
    {
        Guard.IsNotNull(smartDetectionInfo);
        SmartDetectionInfo = smartDetectionInfo;
    }

    internal SmartDetectedTool? SmartDetectionInfo { get; }
}
