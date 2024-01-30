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
    event EventHandler? ChildrenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal sealed class UISettingGroup : UISetting, IUISettingGroup
{
    private IUIElement[]? _children;

    internal UISettingGroup(string? id)
        : base(id)
    {
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        if (InteractiveElement is not null)
        {
            yield return InteractiveElement;
        }

        if (_children is not null)
        {
            foreach (IUIElement child in _children)
            {
                if (child is not null)
                {
                    yield return child;
                }
            }
        }
    }

    /// <inheritdoc/>
    public IUIElement[]? Children
    {
        get => _children;
        internal set => SetPropertyValue(ref _children, value, ChildrenChanged);
    }

    /// <inheritdoc/>
    public bool ChildrenAreAllSettings { get; internal set; }

    /// <inheritdoc/>
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
    /// Sets the <see cref="IUISetting.Description"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="description">The description.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup Description(this IUISettingGroup element, string? description)
    {
        ((UISettingGroup)element).Description = description;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Icon"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="fontName">The font name.</param>
    /// <param name="glyph">The glyph character.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup Icon(this IUISettingGroup element, string fontName, char glyph)
    {
        ((UISettingGroup)element).Icon = Icon(fontName, glyph);
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.InteractiveElement"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="uiElement">The interactive UI element.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup InteractiveElement(this IUISettingGroup element, IUIElement? uiElement)
    {
        ((UISettingGroup)element).InteractiveElement = uiElement;
        return element;
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISettingGroup"/>.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changed.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup Handle(
        this IUISettingGroup element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        Func<bool, ValueTask>? onToggled = null)
    {
        return (IUISettingGroup)((IUISetting)element).Handle(settingsProvider, settingDefinition, onToggled);
    }

    /// <summary>
    /// Sets a <see cref="IUISelectDropDownList"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="onOptionSelected">(optional) A method to invoke when the setting value changed.</param>
    /// <param name="dropDownListItems">(optional) A list of items to be displayed in the drop down list. <see cref="IUIDropDownListItem.Value"/> should be of type <typeparamref name="T"/>.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup Handle<T>(
        this IUISettingGroup element,
        ISettingsProvider settingsProvider,
        SettingDefinition<T> settingDefinition,
        Func<T, ValueTask>? onOptionSelected,
        params IUIDropDownListItem[] dropDownListItems)
        where T : struct, IConvertible
    {
        return (IUISettingGroup)((IUISetting)element).Handle(settingsProvider, settingDefinition, onOptionSelected, dropDownListItems);
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISettingGroup"/>.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changed.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup Handle(
        this IUISettingGroup element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        Action<bool>? onToggled = null)
    {
        return (IUISettingGroup)((IUISetting)element).Handle(settingsProvider, settingDefinition, (value) =>
        {
            onToggled?.Invoke(value);
            return ValueTask.CompletedTask;
        });
    }

    /// <summary>
    /// Sets a <see cref="IUISelectDropDownList"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="onOptionSelected">(optional) A method to invoke when the setting value changed.</param>
    /// <param name="dropDownListItems">(optional) A list of items to be displayed in the drop down list. <see cref="IUIDropDownListItem.Value"/> should be of type <typeparamref name="T"/>.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup Handle<T>(
        this IUISettingGroup element,
        ISettingsProvider settingsProvider,
        SettingDefinition<T> settingDefinition,
        Action<T>? onOptionSelected,
        params IUIDropDownListItem[] dropDownListItems)
        where T : struct, IConvertible
    {
        return (IUISettingGroup)((IUISetting)element).Handle(settingsProvider, settingDefinition, (value) =>
        {
            onOptionSelected?.Invoke(value);
            return ValueTask.CompletedTask;
        }, dropDownListItems);
    }

    /// <summary>
    /// Sets the children to be displayed in the group.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="children">The child elements.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup WithChildren(this IUISettingGroup element, params IUIElement[] children)
    {
        var settingGroup = (UISettingGroup)element;
        settingGroup.ChildrenAreAllSettings = children.All(c => c is IUISetting);
        settingGroup.Children = children;
        return element;
    }

    /// <summary>
    /// Sets the children to be displayed in the group.
    /// </summary>
    /// <param name="element">The <see cref="IUISettingGroup"/> element.</param>
    /// <param name="settings">The settings to be displayed in the group.</param>
    /// <returns>The updated <see cref="IUISettingGroup"/> element.</returns>
    public static IUISettingGroup WithSettings(this IUISettingGroup element, params IUISetting[] settings)
    {
        var settingGroup = (UISettingGroup)element;
        settingGroup.ChildrenAreAllSettings = true;
        settingGroup.Children = settings;
        return element;
    }
}
