using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.UI.ViewModels;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
internal sealed partial class ToolPageViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _headerText = string.Empty;

    internal void Load(GuiToolViewItem guiToolViewItem)
    {
        Guard.IsNotNull(guiToolViewItem);

        HeaderText = guiToolViewItem.ToolInstance.LongDisplayTitle;
    }
}
