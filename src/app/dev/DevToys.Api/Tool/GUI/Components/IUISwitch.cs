using DevToys.Api.Strings.ApiText;

namespace DevToys.Api;

/// <summary>
/// A component that represents a changeable switch.
/// </summary>
public interface IUISwitch : IUIElement
{
    /// <summary>
    /// Gets the current state of the switch.
    /// </summary>
    bool IsOn { get; }

    /// <summary>
    /// Gets the text to display when the <see cref="IsOn"/> is true. Default is "On".
    /// </summary>
    string? OnText { get; }

    /// <summary>
    /// Gets the text to display when the <see cref="IsOn"/> is false. Default is "Off".
    /// </summary>
    string? OffText { get; }

    /// <summary>
    /// Raised when <see cref="OnText"/> is changed.
    /// </summary>
    event EventHandler? OnTextChanged;

    /// <summary>
    /// Raised when <see cref="OffText"/> is changed.
    /// </summary>
    event EventHandler? OffTextChanged;

    /// <summary>
    /// Raised when <see cref="IsOn"/> is changed.
    /// </summary>
    event EventHandler? IsOnChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, IsOn = {{{nameof(IsOn)}}}")]
internal sealed class UISwitch : UITitledElement, IUISwitch
{
    private bool _state;
    private string? _onText;
    private string? _offText;

    internal UISwitch(string? id)
        : base(id)
    {
    }

    public bool IsOn
    {
        get => _state;
        internal set
        {
            if (SetPropertyValue(ref _state, value, IsOnChanged))
            {
                ActionOnToggle?.Invoke(_state);
            }
        }
    }

    public string? OnText
    {
        get => _onText;
        internal set => SetPropertyValue(ref _onText, value, OnTextChanged);
    }

    public string? OffText
    {
        get => _offText;
        internal set => SetPropertyValue(ref _offText, value, OffTextChanged);
    }

    internal Func<bool, ValueTask>? ActionOnToggle { get; set; }

    public event EventHandler? OnTextChanged;
    public event EventHandler? OffTextChanged;
    public event EventHandler? IsOnChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a changeable boolean value.
    /// </summary>
    public static IUISwitch Switch()
    {
        return Switch(null);
    }

    /// <summary>
    /// A component that represents a changeable boolean value.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUISwitch Switch(string? id)
    {
        return new UISwitch(id)
            .OnText(ApiText.SwitchOnDefaultText)
            .OffText(ApiText.SwitchOffDefaultText);
    }

    /// <summary>
    /// Sets the <see cref="IUISwitch.OnText"/> of the switch.
    /// </summary>
    public static IUISwitch OnText(this IUISwitch element, string? text)
    {
        ((UISwitch)element).OnText = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISwitch.OffText"/> of the switch.
    /// </summary>
    public static IUISwitch OffText(this IUISwitch element, string? text)
    {
        ((UISwitch)element).OffText = text;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISwitch.IsOn"/> of the switch to true.
    /// </summary>
    public static IUISwitch On(this IUISwitch element)
    {
        ((UISwitch)element).IsOn = true;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISwitch.IsOn"/> of the switch to off.
    /// </summary>
    public static IUISwitch Off(this IUISwitch element)
    {
        ((UISwitch)element).IsOn = false;
        return element;
    }

    /// <summary>
    /// Sets the action to run when toggling the switch.
    /// </summary>
    public static IUISwitch OnToggle(this IUISwitch element, Func<bool, ValueTask>? actionOnToggle)
    {
        ((UISwitch)element).ActionOnToggle = actionOnToggle;
        return element;
    }

    /// <summary>
    /// Sets the action to run when toggling the switch.
    /// </summary>
    public static IUISwitch OnToggle(this IUISwitch element, Action<bool>? actionOnToggle)
    {
        ((UISwitch)element).ActionOnToggle
            = (value) =>
            {
                actionOnToggle?.Invoke(value);
                return ValueTask.CompletedTask;
            };
        return element;
    }
}
