using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Core.Tools;

namespace DevToys.UI.ViewModels;

[Export(typeof(MainWindowViewModel))]
internal sealed class MainWindowViewModel : ObservableRecipient
{
    [ImportingConstructor]
    public MainWindowViewModel(GuiToolProvider guiToolProvider)
    {

    }
}
