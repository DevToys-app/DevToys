using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Api;

namespace DevToys.Core.Tools.ViewItems;

/// <summary>
/// Represents a group or category in the main menu.
/// </summary>
[DebuggerDisplay($"DisplayTitle = {{{nameof(DisplayTitle)}}}")]
public sealed class GroupViewItem : ObservableObject
{
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
        bool isExpandedByDefault = false)
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
        IsExpandedByDefault = isExpandedByDefault;
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
    public bool IsExpandedByDefault { get; }
}
