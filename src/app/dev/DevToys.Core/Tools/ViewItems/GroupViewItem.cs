using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DevToys.Api;

namespace DevToys.Core.Tools.ViewItems;

/// <summary>
/// Represents a group or category in the main menu.
/// </summary>
[DebuggerDisplay($"DisplayTitle = {{{nameof(DisplayTitle)}}}")]
public sealed class GroupViewItem : ObservableObject
{
    private TaskCompletionSource? _tcs;
    private bool _childItemJustGotSelected;

    internal GroupViewItem(
        string internalName,
        GuiToolGroup groupInfo,
        ObservableCollection<GuiToolViewItem>? children = null)
        : this(
              internalName,
              groupInfo.IconFontName,
              groupInfo.IconGlyph,
              groupInfo.DisplayTitle,
              groupInfo.AccessibleName,
              children)
    {
    }

    internal GroupViewItem(
        string internalName,
        string iconFontName,
        string iconGlyph,
        string displayTitle,
        string accessibleName,
        ObservableCollection<GuiToolViewItem>? children = null,
        bool menuItemShouldBeExpandedByDefault = false)
    {
        Guard.IsNotNullOrWhiteSpace(internalName);
        Guard.IsNotNullOrWhiteSpace(iconFontName);
        Guard.IsNotNullOrWhiteSpace(iconGlyph);
        Guard.IsNotNullOrWhiteSpace(displayTitle);
        InternalName = internalName;
        DisplayTitle = displayTitle;
        AccessibleName = accessibleName ?? string.Empty;
        IconFontName = iconFontName;
        IconGlyph = iconGlyph;
        Children = children;
        MenuItemShouldBeExpandedByDefault = menuItemShouldBeExpandedByDefault;

        if (children is not null)
        {
            children.CollectionChanged += Children_CollectionChanged;
        }
    }

    /// <summary>
    /// Gets the internal name of the group.
    /// </summary>
    public string InternalName { get; }

    /// <summary>
    /// Gets the long title to display in the menu.
    /// </summary>
    public string DisplayTitle { get; }

    /// <summary>
    /// Gets the name of the group that will be told to the user when using screen reader.
    /// </summary>
    public string AccessibleName { get; }

    /// <summary>
    /// Gets the name of the font to use to display the <see cref="IconGlyph"/>.
    /// </summary>
    public string IconFontName { get; }

    /// <summary>
    /// Gets a glyph for the icon of the group.
    /// </summary>
    public string IconGlyph { get; }

    /// <summary>
    /// Gets all the children items of this group.
    /// </summary>
    public ObservableCollection<GuiToolViewItem>? Children { get; }

    /// <summary>
    /// Gets whether the group should be expanded by default.
    /// </summary>
    public bool MenuItemShouldBeExpandedByDefault { get; }

    /// <summary>
    /// Gets whether the group should be expanded.
    /// </summary>
    public bool MenuItemShouldBeExpanded
    {
        get
        {
            if (Children is not null)
            {
                if (Children.Any(item => item.IsRecommended))
                {
                    return true;
                }
            }

            if (_childItemJustGotSelected)
            {
                Task.Delay(1000)
                    .ContinueWith(_ =>
                    {
                        _childItemJustGotSelected = false;
                    });
                return true;
            }

            return false;
        }
    }

    private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (object? item in e.OldItems)
            {
                if (item is GuiToolViewItem guiToolViewItem)
                {
                    guiToolViewItem.PropertyChanged -= Child_PropertyChanged;
                    guiToolViewItem.GotSelected -= Child_GotSelected;
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (object? item in e.NewItems)
            {
                if (item is GuiToolViewItem guiToolViewItem)
                {
                    guiToolViewItem.PropertyChanged += Child_PropertyChanged;
                    guiToolViewItem.GotSelected += Child_GotSelected;
                }
            }
        }
    }

    private void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GuiToolViewItem.IsRecommended))
        {
            OnPropertyChanged(nameof(MenuItemShouldBeExpanded));
        }
    }

    private void Child_GotSelected(object? sender, EventArgs e)
    {
        _childItemJustGotSelected = true;
        OnPropertyChanged(nameof(MenuItemShouldBeExpanded));
    }
}
