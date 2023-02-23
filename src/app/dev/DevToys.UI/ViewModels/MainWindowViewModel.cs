using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            if (value is null && _selectedMenuItem is not null)
            {
                // Somehow, the NavigationView end up with no selected item. An example of scenario where it can happen is when
                // the selected item is a favorite one and the user remove it from the favorites.
                // What we do in this scenario is that we try to find the equivalent item corresponding to the selected item we had.
                // If we don't find anything, let's selected the first item in the menu.
                SetProperty(ref _selectedMenuItem, GetBestMenuItemToSelect(_selectedMenuItem));
            }
            else
            {
                SetProperty(ref _selectedMenuItem, value);
            }
            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(IsSelectedMenuItemSupportFavorite));
            OnPropertyChanged(nameof(IsSelectedMenuItemAFavoriteTool));
        }
    }

    /// <summary>
    /// Gets the text to show in the header of the app according to the <see cref="SelectedMenuItem"/>.
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

    /// <summary>
    /// Indicates whether the <see cref="SelectedMenuItem"/> can be added or removed from favorites.
    /// </summary>
    internal bool IsSelectedMenuItemSupportFavorite => SelectedMenuItem is GuiToolViewItem guiToolViewItem && !guiToolViewItem.ToolInstance.NotFavorable;

    /// <summary>
    /// Indicates whether the <see cref="SelectedMenuItem"/> is a favorite tool or not.
    /// </summary>
    internal bool IsSelectedMenuItemAFavoriteTool => SelectedMenuItem is GuiToolViewItem guiToolViewItem && _guiToolProvider.GetToolIsFavorite(guiToolViewItem.ToolInstance);

    /// <summary>
    /// Toggles the favorite status of the <see cref="SelectedMenuItem"/>.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectedMenuItemFavorite()
    {
        if (SelectedMenuItem is GuiToolViewItem guiToolViewItem)
        {
            _guiToolProvider.SetToolIsFavorite(guiToolViewItem.ToolInstance, !_guiToolProvider.GetToolIsFavorite(guiToolViewItem.ToolInstance));
            OnPropertyChanged(nameof(IsSelectedMenuItemAFavoriteTool));
        }
    }

    private INotifyPropertyChanged GetBestMenuItemToSelect(INotifyPropertyChanged currentSelectedMenuItem)
    {
        Guard.IsNotEmpty((IReadOnlyList<INotifyPropertyChanged>)HeaderAndBodyToolViewItems);
        Guard.IsNotNull(currentSelectedMenuItem);

        if (currentSelectedMenuItem is GuiToolViewItem guiToolViewItem)
        {
            GuiToolViewItem? itemToSelect = _guiToolProvider.GetViewItemFromTool(guiToolViewItem.ToolInstance).FirstOrDefault();
            if (itemToSelect is not null)
            {
                return itemToSelect;
            }
        }

        return HeaderAndBodyToolViewItems[0];
    }
}
