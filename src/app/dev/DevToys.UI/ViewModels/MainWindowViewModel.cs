using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.UI.ViewModels;

[Export(typeof(MainWindowViewModel))]
internal sealed class MainWindowViewModel : ObservableRecipient
{
    private readonly GuiToolProvider _guiToolProvider;

    [ImportingConstructor]
    public MainWindowViewModel(GuiToolProvider guiToolProvider)
    {
        _guiToolProvider = guiToolProvider;
    }

    /// <summary>
    /// Gets a hierarchical list containing all the tools available, ordered, to display in the top and body menu.
    /// This includes "All tools" menu item, recents and favorites.
    /// </summary>
    public ReadOnlyObservableCollection<INotifyPropertyChanged> HeaderAndBodyToolViewItems => _guiToolProvider.HeaderAndBodyToolViewItems;

    /// <summary>
    /// Gets a flat list containing all the footer tools available, ordered.
    /// </summary>
    public ReadOnlyObservableCollection<GuiToolViewItem> FooterToolViewItems => _guiToolProvider.FooterToolViewItems;
}
