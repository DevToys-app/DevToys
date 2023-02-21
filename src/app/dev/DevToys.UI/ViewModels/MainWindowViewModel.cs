using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.UI.ViewModels;

[Export(typeof(MainWindowViewModel))]
internal sealed partial class MainWindowViewModel : ObservableRecipient
{
    private readonly GuiToolProvider _guiToolProvider;

    private INotifyPropertyChanged? _selectedMenuItem;

    [ImportingConstructor]
    public MainWindowViewModel(GuiToolProvider guiToolProvider)
    {
        _guiToolProvider = guiToolProvider;
    }

    /// <summary>
    /// Gets a hierarchical list containing all the tools available, ordered, to display in the top and body menu.
    /// This includes "All tools" menu item, recents and favorites.
    /// </summary>
    internal ReadOnlyObservableCollection<INotifyPropertyChanged> HeaderAndBodyToolViewItems => _guiToolProvider.HeaderAndBodyToolViewItems;

    /// <summary>
    /// Gets a flat list containing all the footer tools available, ordered.
    /// </summary>
    internal ReadOnlyObservableCollection<GuiToolViewItem> FooterToolViewItems => _guiToolProvider.FooterToolViewItems;

    // Can't use CommunityToolkit.MVVM due to https://github.com/dotnet/roslyn/issues/57239#issuecomment-1437895948
    /// <summary>
    /// Gets or sets the selected menu item in the NavitationView.
    /// </summary>
    internal INotifyPropertyChanged? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            SetProperty(ref _selectedMenuItem, value);
            OnPropertyChanged(nameof(HeaderText));
        }
    }

    /// <summary>
    /// Gets the text to show in the header of the app.
    /// </summary>
    internal string? HeaderText
    {
        get
        {
            if (SelectedMenuItem is GuiToolViewItem guiToolViewItem)
            {
                return guiToolViewItem.ToolInstance.LongDisplayTitle;
            }
            else if (SelectedMenuItem is GroupViewItem groupViewItem)
            {
                return groupViewItem.DisplayTitle;
            }

            return string.Empty;
        }
    }
}
