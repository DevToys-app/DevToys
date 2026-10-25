namespace DevToys.Api;

/// <summary>
/// A component that represents a check box
/// </summary>
public interface IUICheckBox : IUIElement
{
    /// <summary>
    /// Gets the text displayed next to the check box
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets whether the check box is checked
    /// </summary>
    bool IsChecked { get; }

    /// <summary>
    /// Raised when <see cref="Text"/> is changed
    /// </summary>
    event EventHandler? TextChanged;

    /// <summary>
    /// Raised when <see cref="IsChecked"/> is changed
    /// </summary>
    event EventHandler? IsCheckedChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, IsChecked = {{{nameof(IsChecked)}}}")]
internal sealed class UICheckBox : UIElement, IUICheckBox
{
    private string? _text;
    private bool _isChecked;

    internal UICheckBox(string? id)
        : base(id)
    {
    }

    public string? Text
    {
        get => _text;
        internal set => SetPropertyValue(ref _text, value, TextChanged);
    }

    public bool IsChecked
    {
        get => _isChecked;
        internal set
        {
            if (SetPropertyValue(ref _isChecked, value, IsCheckedChanged))
            {
                ActionOnToggle?.Invoke(_isChecked);
            }
        }
    }

    internal Func<bool, ValueTask>? ActionOnToggle { get; set; }

    public event EventHandler? TextChanged;
    public event EventHandler? IsCheckedChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Creates a check box
    /// </summary>
    public static IUICheckBox CheckBox()
    {
        return CheckBox(null);
    }

    /// <summary>
    /// Creates a check box
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element</param>
    public static IUICheckBox CheckBox(string? id)
    {
        return new UICheckBox(id);
    }

    /// <summary>
    /// Sets the text displayed next to the check box
    /// </summary>
    public static IUICheckBox Text(this IUICheckBox element, string? text)
    {
        ((UICheckBox)element).Text = text;
        return element;
    }

    /// <summary>
    /// Checks the check box
    /// </summary>
    public static IUICheckBox Check(this IUICheckBox element)
    {
        ((UICheckBox)element).IsChecked = true;
        return element;
    }

    /// <summary>
    /// Unchecks the check box
    /// </summary>
    public static IUICheckBox Uncheck(this IUICheckBox element)
    {
        ((UICheckBox)element).IsChecked = false;
        return element;
    }

    /// <summary>
    /// Sets the action to run when the checked state changes
    /// </summary>
    public static IUICheckBox OnToggle(this IUICheckBox element, Func<bool, ValueTask>? actionOnToggle)
    {
        ((UICheckBox)element).ActionOnToggle = actionOnToggle;
        return element;
    }

    /// <summary>
    /// Sets the action to run when the checked state changes
    /// </summary>
    public static IUICheckBox OnToggle(this IUICheckBox element, Action<bool>? actionOnToggle)
    {
        ((UICheckBox)element).ActionOnToggle
            = (value) =>
            {
                actionOnToggle?.Invoke(value);
                return ValueTask.CompletedTask;
            };
        return element;
    }
}
