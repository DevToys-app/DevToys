using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToys.Core.Tools.ViewItems;

/// <summary>
/// Represents a group or category in the main menu.
/// </summary>
[DebuggerDisplay($"DisplayTitle = {{{nameof(DisplayTitle)}}}")]
public sealed class GroupViewItem : ObservableObject, IGroup
{
    private bool _childItemJustGotSelected;
    private bool _isExpanded;

    internal GroupViewItem(
        string internalName,
        GuiToolGroup groupInfo)
        : this(
              internalName,
              groupInfo.IconFontName,
              groupInfo.IconGlyph,
              groupInfo.DisplayTitle,
              groupInfo.AccessibleName,
              new())
    {
    }

    internal GroupViewItem(
        string internalName,
        string iconFontName,
        char iconGlyph,
        string displayTitle,
        string accessibleName,
        ObservableCollection<GuiToolViewItem>? children = null,
        bool menuItemShouldBeExpandedByDefault = false)
    {
        Guard.IsNotNullOrWhiteSpace(internalName);
        Guard.IsNotNullOrWhiteSpace(iconFontName);
        Guard.IsNotNullOrWhiteSpace(displayTitle);
        InternalName = internalName;
        DisplayTitle = displayTitle;
        AccessibleName = accessibleName ?? string.Empty;
        IconFontName = iconFontName;
        IconGlyph = iconGlyph;
        Children = children;
        ChildrenItems = Children;
        GroupShouldBeExpandedByDefaultInUI = menuItemShouldBeExpandedByDefault;

        if (children is not null)
        {
            children.CollectionChanged += Children_CollectionChanged;
            foreach (object? item in children)
            {
                if (item is GuiToolViewItem guiToolViewItem)
                {
                    guiToolViewItem.PropertyChanged += Child_PropertyChanged;
                    guiToolViewItem.GotSelected += Child_GotSelected;
                }
            }
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
    public char IconGlyph { get; }

    /// <summary>
    /// Gets all the children items of this group.
    /// </summary>
    public ObservableCollection<GuiToolViewItem>? Children { get; }

    /// <summary>
    /// Gets all the children items of this group.
    /// </summary>
    public IEnumerable<IItem>? ChildrenItems { get; set; }

    /// <summary>
    /// Gets whether the group should be expanded by default.
    /// </summary>
    public bool GroupShouldBeExpandedByDefaultInUI { get; }

    /// <summary>
    /// Gets whether the group should be expanded.
    /// </summary>
    public bool GroupShouldBeExpandedInUI
    {
        get
        {
            if (IsAnyChildrenRecommended())
            {
                return true;
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

    /// <summary>
    /// Gets or sets whether the group is expanded in the UI.
    /// </summary>
    public bool GroupIsExpandedInUI
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
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
        if (e.PropertyName == nameof(GuiToolViewItem.IsRecommended) && IsAnyChildrenRecommended())
        {
            OnPropertyChanged(nameof(GroupShouldBeExpandedInUI));
        }
    }

    private void Child_GotSelected(object? sender, EventArgs e)
    {
        _childItemJustGotSelected = true;
        OnPropertyChanged(nameof(GroupShouldBeExpandedInUI));
    }

    private bool IsAnyChildrenRecommended()
    {
        if (Children is not null)
        {
            if (Children.Any(item => item.IsRecommended))
            {
                return true;
            }
        }

        return false;
    }
}
