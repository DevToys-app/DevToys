namespace DevToys.Api;

/// <summary>
/// A component that represents a group of settings, with a title, description and an icon.
/// </summary>
public interface IUISettingGroup : IUISetting
{
    /// <summary>
    /// Gets the list of child elements.
    /// </summary>
    IUIElement[]? Children { get; }

    /// <summary>
    /// Gets whether element in <see cref="Children"/> are all of type <see cref="IUISetting"/>.
    /// </summary>
    bool ChildrenAreAllSettings { get; }

    /// <summary>
    /// Raised when <see cref="Children"/> is changed.
    /// </summary>
    public event EventHandler? ChildrenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal sealed class UISettingGroup : UISetting, IUISettingGroup
{
    private IUIElement[]? _children;

    internal UISettingGroup(string? id)
        : base(id)
    {
    }

    public IUIElement[]? Children
    {
        get => _children;
        internal set
        {
            _children = value;
            ChildrenChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool ChildrenAreAllSettings { get; internal set; }

    public event EventHandler? ChildrenChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a group of settings, with a title, description and an icon.
    /// </summary>
    public static IUISettingGroup SettingGroup()
    {
        return SettingGroup(null);
    }

    /// <summary>
    /// A component that represents a group of settings, with a title, description and an icon.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISettingGroup SettingGroup(string? id)
    {
        return new UISettingGroup(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISettingGroup.Description"/> of the setting.
    /// </summary>
    public static IUISettingGroup Description(this IUISettingGroup element, string? text)
    {
        ((UISettingGroup)element).Description = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISettingGroup.Icon"/> of the setting.
    /// </summary>
    public static IUISettingGroup Icon(this IUISettingGroup element, string fontName, string glyph)
    {
        ((UISettingGroup)element).Icon = Icon(fontName, glyph);
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISettingGroup.InteractiveElement"/> of the setting.
    /// </summary>
    public static IUISettingGroup InteractiveElement(this IUISettingGroup element, IUIElement? uiElement)
    {
        ((UISettingGroup)element).InteractiveElement = uiElement;
        return element;
    }

    /// <summary>
    /// Set the children to be displayed in the group.
    /// </summary>
    public static IUISettingGroup WithChildren(this IUISettingGroup element, params IUIElement[] children)
    {
        var settingGroup = (UISettingGroup)element;
        settingGroup.ChildrenAreAllSettings = children.All(c => c is IUISetting);
        settingGroup.Children = children;
        return element;
    }

    /// <summary>
    /// Set the children to be displayed in the group.
    /// </summary>
    public static IUISettingGroup WithSettings(this IUISettingGroup element, params IUISetting[] settings)
    {
        var settingGroup = (UISettingGroup)element;
        settingGroup.ChildrenAreAllSettings = true;
        settingGroup.Children = settings;
        return element;
    }
}
