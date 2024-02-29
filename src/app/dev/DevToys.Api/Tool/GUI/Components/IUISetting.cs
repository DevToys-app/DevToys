namespace DevToys.Api;

/// <summary>
/// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
/// </summary>
public interface IUISetting : IUITitledElementWithChildren
{
    /// <summary>
    /// Gets the description of the setting.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the description of the state of the setting.
    /// </summary>
    string? StateDescription { get; }

    /// <summary>
    /// Gets the icon of the setting.
    /// </summary>
    IUIIcon? Icon { get; }

    /// <summary>
    /// Gets the <see cref="IUIElement"/> that represents the interactive part of the setting.
    /// </summary>
    IUIElement? InteractiveElement { get; }

    /// <summary>
    /// Raised when <see cref="Description"/> is changed.
    /// </summary>
    event EventHandler? DescriptionChanged;

    /// <summary>
    /// Raised when <see cref="StateDescription"/> is changed.
    /// </summary>
    event EventHandler? StateDescriptionChanged;

    /// <summary>
    /// Raised when <see cref="Icon"/> is changed.
    /// </summary>
    event EventHandler? IconChanged;

    /// <summary>
    /// Raised when <see cref="InteractiveElement"/> is changed.
    /// </summary>
    event EventHandler? InteractiveElementChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal class UISetting : UITitledElementWithChildren, IUISetting
{
    private string? _description;
    private string? _stateDescription;
    private IUIIcon? _icon;
    private IUIElement? _interactiveElement;

    internal UISetting(string? id)
        : base(id)
    {
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        if (InteractiveElement is not null)
        {
            yield return InteractiveElement;
        }
    }

    /// <inheritdoc/>
    public string? Description
    {
        get => _description;
        internal set => SetPropertyValue(ref _description, value, DescriptionChanged);
    }

    /// <inheritdoc/>
    public string? StateDescription
    {
        get => _stateDescription;
        internal set => SetPropertyValue(ref _stateDescription, value, StateDescriptionChanged);
    }

    /// <inheritdoc/>
    public IUIIcon? Icon
    {
        get => _icon;
        internal set => SetPropertyValue(ref _icon, value, IconChanged);
    }

    /// <inheritdoc/>
    public IUIElement? InteractiveElement
    {
        get => _interactiveElement;
        internal set => SetPropertyValue(ref _interactiveElement, value, InteractiveElementChanged);
    }

    /// <inheritdoc/>
    public event EventHandler? DescriptionChanged;

    /// <inheritdoc/>
    public event EventHandler? StateDescriptionChanged;

    /// <inheritdoc/>
    public event EventHandler? IconChanged;

    /// <inheritdoc/>
    public event EventHandler? InteractiveElementChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <returns>The created <see cref="IUISetting"/> instance.</returns>
    public static IUISetting Setting()
    {
        return Setting(null);
    }

    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <returns>The created <see cref="IUISetting"/> instance.</returns>
    public static IUISetting Setting(string? id)
    {
        return new UISetting(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Description"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="text">The description text.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    public static IUISetting Description(this IUISetting element, string? text)
    {
        ((UISetting)element).Description = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.StateDescription"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="text">The state description text.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    public static IUISetting StateDescription(this IUISetting element, string? text)
    {
        ((UISetting)element).StateDescription = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Icon"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="fontName">The font name of the icon.</param>
    /// <param name="glyph">The glyph of the icon.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    public static IUISetting Icon(this IUISetting element, string fontName, char glyph)
    {
        ((UISetting)element).Icon = Icon(fontName, glyph);
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.InteractiveElement"/> of the setting.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="uiElement">The interactive UI element.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    public static IUISetting InteractiveElement(this IUISetting element, IUIElement? uiElement)
    {
        ((UISetting)element).InteractiveElement = uiElement;
        return element;
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate with this <see cref="IUISetting"/>.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changes.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    /// <remarks>
    /// The binding with the setting is going in both directions. When the setting is set, the switch is updated. When the switch is toggled, the setting is updated.
    /// </remarks>
    public static IUISetting Handle(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        Func<bool, ValueTask>? onToggled = null)
    {
        return Handle(
            element,
            settingsProvider,
            settingDefinition,
            stateDescriptionWhenOn: null,
            stateDescriptionWhenOff: null,
            ignoreStateDescription: true,
            onToggled);
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate with this <see cref="IUISetting"/>.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changes.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    /// <remarks>
    /// The binding with the setting is going in both directions. When the setting is set, the switch is updated. When the switch is toggled, the setting is updated.
    /// </remarks>
    public static IUISetting Handle(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        Action<bool> onToggled)
    {
        return Handle(
            element,
            settingsProvider,
            settingDefinition,
            stateDescriptionWhenOn: null,
            stateDescriptionWhenOff: null,
            ignoreStateDescription: true,
            (value) =>
            {
                onToggled?.Invoke(value);
                return ValueTask.CompletedTask;
            });
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="stateDescriptionWhenOn">The <see cref="IUISetting.StateDescription"/> to use when the option is On.</param>
    /// <param name="stateDescriptionWhenOff">The <see cref="IUISetting.StateDescription"/> to use when the option is Off.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changed.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    /// <remarks>
    /// The binding with the setting is going in both directions. When the setting is set, the switch is updated. When the switch is toggled, the setting is updated.
    /// </remarks>
    public static IUISetting Handle(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        string? stateDescriptionWhenOn,
        string? stateDescriptionWhenOff,
        Func<bool, ValueTask>? onToggled = null)
    {
        return Handle(
            element,
            settingsProvider,
            settingDefinition,
            stateDescriptionWhenOn,
            stateDescriptionWhenOff,
            ignoreStateDescription: false,
            onToggled);
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="stateDescriptionWhenOn">The <see cref="IUISetting.StateDescription"/> to use when the option is On.</param>
    /// <param name="stateDescriptionWhenOff">The <see cref="IUISetting.StateDescription"/> to use when the option is Off.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changed.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    /// <remarks>
    /// The binding with the setting is going in both directions. When the setting is set, the switch is updated. When the switch is toggled, the setting is updated.
    /// </remarks>
    public static IUISetting Handle(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        string? stateDescriptionWhenOn,
        string? stateDescriptionWhenOff,
        Action<bool>? onToggled)
    {
        return Handle(
            element,
            settingsProvider,
            settingDefinition,
            stateDescriptionWhenOn,
            stateDescriptionWhenOff,
            ignoreStateDescription: false,
            (value) =>
            {
                onToggled?.Invoke(value);
                return ValueTask.CompletedTask;
            });
    }

    /// <summary>
    /// Sets a <see cref="IUISelectDropDownList"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="onOptionSelected">(optional) A method to invoke when the setting value changed.</param>
    /// <param name="dropDownListItems">(optional) A list of items to be displayed in the drop down list. <see cref="IUIDropDownListItem.Value"/> should be of type <typeparamref name="T"/>.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    /// <remarks>
    /// The binding with the setting is going in both directions. When the setting is changed, the drop down list is updated. When the drop down list is changed, the setting is updated.
    /// </remarks>
    public static IUISetting Handle<T>(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<T> settingDefinition,
        Action<T>? onOptionSelected,
        params IUIDropDownListItem[] dropDownListItems)
    {
        return Handle(
            element,
            settingsProvider,
            settingDefinition,
            (value) =>
            {
                onOptionSelected?.Invoke(value);
                return ValueTask.CompletedTask;
            },
            dropDownListItems);
    }

    /// <summary>
    /// Sets a <see cref="IUISelectDropDownList"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="element">The <see cref="IUISetting"/> instance.</param>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="onOptionSelected">(optional) A method to invoke when the setting value changed.</param>
    /// <param name="dropDownListItems">(optional) A list of items to be displayed in the drop down list. <see cref="IUIDropDownListItem.Value"/> should be of type <typeparamref name="T"/>.</param>
    /// <returns>The updated <see cref="IUISetting"/> instance.</returns>
    /// <remarks>
    /// The binding with the setting is going in both directions. When the setting is changed, the drop down list is updated. When the drop down list is changed, the setting is updated.
    /// </remarks>
    public static IUISetting Handle<T>(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<T> settingDefinition,
        Func<T, ValueTask>? onOptionSelected,
        params IUIDropDownListItem[] dropDownListItems)
    {
        bool typeIsEnum = typeof(T).IsEnum;

        var settingElement = (UISetting)element;

        IUISelectDropDownList dropDownList
            = SelectDropDownList()
                .WithItems(dropDownListItems);

        T currentSettingValue = settingsProvider.GetSetting(settingDefinition);

        dropDownList.Select(FindItemInDropDownList<T>(dropDownList, currentSettingValue));

        dropDownList.OnItemSelected((IUIDropDownListItem? item) =>
        {
            Guard.IsNotNull(item);
            Guard.IsNotNull(item.Value);
            Guard.IsOfType<T>(item.Value);

            settingsProvider.SetSetting(settingDefinition, (T)item.Value);
            if (onOptionSelected is not null)
            {
                return onOptionSelected.Invoke((T)item.Value);
            }
            return ValueTask.CompletedTask;
        });

        settingsProvider.SettingChanged += (object? sender, SettingChangedEventArgs e) =>
        {
            if (e.SettingName == settingDefinition.Name)
            {
                IUIDropDownListItem? itemToSelect = FindItemInDropDownList<T>(dropDownList, e.NewValue);

                if (itemToSelect is not null && dropDownList.SelectedItem != itemToSelect)
                {
                    dropDownList.Select(itemToSelect);
                }
            }
        };

        settingElement.InteractiveElement(dropDownList);
        return element;
    }

    private static IUISetting Handle(
        this IUISetting element,
        ISettingsProvider settingsProvider,
        SettingDefinition<bool> settingDefinition,
        string? stateDescriptionWhenOn,
        string? stateDescriptionWhenOff,
        bool ignoreStateDescription,
        Func<bool, ValueTask>? onToggled)
    {
        var settingElement = (UISetting)element;

        IUISwitch toggleSwitch = Switch();
        if (settingsProvider.GetSetting(settingDefinition))
        {
            toggleSwitch.On();
            if (!ignoreStateDescription)
            {
                settingElement.StateDescription = stateDescriptionWhenOn;
            }
        }
        else
        {
            toggleSwitch.Off();
            if (!ignoreStateDescription)
            {
                settingElement.StateDescription = stateDescriptionWhenOff;
            }
        }

        toggleSwitch.OnToggle((bool state) =>
        {
            settingsProvider.SetSetting(settingDefinition, state);

            if (!ignoreStateDescription)
            {
                if (state)
                {
                    settingElement.StateDescription = stateDescriptionWhenOn;
                }
                else
                {
                    settingElement.StateDescription = stateDescriptionWhenOff;
                }
            }

            if (onToggled is not null)
            {
                return onToggled.Invoke(state);
            }
            return ValueTask.CompletedTask;
        });

        settingsProvider.SettingChanged += (object? sender, SettingChangedEventArgs e) =>
        {
            if (e.SettingName == settingDefinition.Name)
            {
                Guard.IsOfType<bool>(e.NewValue!);
                bool settingValue = (bool)e.NewValue!;
                if (settingValue)
                {
                    if (!toggleSwitch.IsOn)
                    {
                        toggleSwitch.On();
                        if (!ignoreStateDescription)
                        {
                            settingElement.StateDescription = stateDescriptionWhenOn;
                        }
                    }
                }
                else
                {
                    if (toggleSwitch.IsOn)
                    {
                        toggleSwitch.Off();
                        if (!ignoreStateDescription)
                        {
                            settingElement.StateDescription = stateDescriptionWhenOff;
                        }
                    }
                }
            }
        };

        settingElement.InteractiveElement(toggleSwitch);
        return element;
    }

    private static IUIDropDownListItem? FindItemInDropDownList<T>(IUISelectDropDownList element, object? value)
    {
        return element.Items?.FirstOrDefault((IUIDropDownListItem item) =>
        {
            if (item.Value is null)
            {
                if (value is null)
                {
                    return true;
                }

                return false;
            }

            Guard.IsOfType<T>(item.Value);
            if (value is not null)
            {
                Guard.IsOfType<T>(value);
            }

            return item.Value.Equals(value);
        });
    }
}
