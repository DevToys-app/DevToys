using System;

namespace DevToys.Api;

/// <summary>
/// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
/// </summary>
public interface IUISetting : IUITitledElement
{
    /// <summary>
    /// Gets the description of the setting.
    /// </summary>
    string? Description { get; }

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
    public event EventHandler? DescriptionChanged;

    /// <summary>
    /// Raised when <see cref="Icon"/> is changed.
    /// </summary>
    public event EventHandler? IconChanged;

    /// <summary>
    /// Raised when <see cref="InteractiveElement"/> is changed.
    /// </summary>
    public event EventHandler? InteractiveElementChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal class UISetting : UITitledElement, IUISetting
{
    private string? _description;
    private IUIIcon? _icon;
    private IUIElement? _interactiveElement;

    internal UISetting(string? id)
        : base(id)
    {
    }

    public string? Description
    {
        get => _description;
        internal set
        {
            _description = value;
            DescriptionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IUIIcon? Icon
    {
        get => _icon;
        internal set
        {
            _icon = value;
            IconChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IUIElement? InteractiveElement
    {
        get => _interactiveElement;
        internal set
        {
            _interactiveElement = value;
            InteractiveElementChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DescriptionChanged;
    public event EventHandler? IconChanged;
    public event EventHandler? InteractiveElementChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    public static IUISetting Setting()
    {
        return Setting(null);
    }

    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISetting Setting(string? id)
    {
        return new UISetting(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Description"/> of the setting.
    /// </summary>
    public static IUISetting Description(this IUISetting element, string? text)
    {
        ((UISetting)element).Description = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.Icon"/> of the setting.
    /// </summary>
    public static IUISetting Icon(this IUISetting element, string fontName, string glyph)
    {
        ((UISetting)element).Icon = Icon(fontName, glyph);
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.InteractiveElement"/> of the setting.
    /// </summary>
    public static IUISetting InteractiveElement(this IUISetting element, IUIElement? uiElement)
    {
        ((UISetting)element).InteractiveElement = uiElement;
        return element;
    }

    /// <summary>
    /// Sets a <see cref="IUISwitch"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="onToggled">(optional) A method to invoke when the setting value changed.</param>
    public static IUISetting Handle(this IUISetting element, ISettingsProvider settingsProvider, SettingDefinition<bool> settingDefinition, Func<bool, ValueTask>? onToggled = null)
    {
        var settingElement = (UISetting)element;

        IUISwitch toggleSwitch = Switch();
        if (settingsProvider.GetSetting(settingDefinition))
        {
            toggleSwitch.On();
        }
        else
        {
            toggleSwitch.Off();
        }

        toggleSwitch.OnToggle((bool state) =>
        {
            settingsProvider.SetSetting(settingDefinition, state);
            if (onToggled is not null)
            {
                return onToggled.Invoke(state);
            }
            return ValueTask.CompletedTask;
        });

        settingElement.InteractiveElement(toggleSwitch);
        return element;
    }

    /// <summary>
    /// Sets a <see cref="IUIDropDownList"/> to <see cref="IUISetting.InteractiveElement"/> and automatically associate the
    /// given <paramref name="settingDefinition"/> to the switch state.
    /// </summary>
    /// <param name="settingsProvider">The settings provider used for handling the given <paramref name="settingDefinition"/>.</param>
    /// <param name="settingDefinition">The definition of the setting to associate to this <see cref="IUISetting"/>.</param>
    /// <param name="onOptionSelected">(optional) A method to invoke when the setting value changed.</param>
    /// <param name="dropDownListItems">(optional) A list of items to be displayed in the drop down list. <see cref="IUIDropDownListItem.Value"/> should be of type <typeparamref name="T"/>.</param>
    public static IUISetting Handle<T>(this IUISetting element, ISettingsProvider settingsProvider, SettingDefinition<T> settingDefinition, Func<T, ValueTask>? onOptionSelected, params IUIDropDownListItem[] dropDownListItems) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            ThrowHelper.ThrowArgumentException($"{nameof(T)} must be an enumerated type.");
        }

        var settingElement = (UISetting)element;

        IUIDropDownList dropDownList
            = DropDownList()
                .WithItems(dropDownListItems);

        T currentSettingValue = settingsProvider.GetSetting(settingDefinition);

        dropDownList.Select(dropDownList.Items?.FirstOrDefault(i => i.Value is T e && e.ToInt32(null) == currentSettingValue.ToInt32(null)));

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

        settingElement.InteractiveElement(dropDownList);
        return element;
    }
}
