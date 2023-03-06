namespace DevToys.Api;

/// <summary>
/// A component that represents a button, which reacts when clicking on it.
/// </summary>
public interface IUIButton : IUIElement
{
    /// <summary>
    /// Gets the text to display in the button.
    /// </summary>
    string? DisplayText { get; }

    /// <summary>
    /// Gets the action to run when the user clicks the button.
    /// </summary>
    Func<ValueTask>? OnClickAction { get; }

    /// <summary>
    /// Raised when <see cref="DisplayTextChanged"/> is changed.
    /// </summary>
    public event EventHandler? DisplayTextChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(DisplayText)}}}")]
internal class UIButton : UIElement, IUIButton
{
    private string? _text;

    internal UIButton(string? id)
        : base(id)
    {
    }

    public string? DisplayText
    {
        get => _text;
        internal set
        {
            _text = value;
            DisplayTextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Func<ValueTask>? OnClickAction { get; internal set; }

    public event EventHandler? DisplayTextChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create component that represents a button, which reacts when clicking on it.
    /// </summary>
    public static IUIButton Button()
    {
        return Button(null);
    }

    /// <summary>
    /// Create component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIButton Button(string? id)
    {
        return new UIButton(id);
    }

    /// <summary>
    /// Sets the <see cref="IUIButton.DisplayText"/> of the button.
    /// </summary>
    public static IUIButton Text(this IUIButton element, string? text)
    {
        ((UIButton)element).DisplayText = text;
        return element;
    }

    /// <summary>
    /// Sets the action to run when clicking on the button.
    /// </summary>
    public static IUIButton OnClick(this IUIButton element, Func<ValueTask>? actionOnClick)
    {
        ((UIButton)element).OnClickAction = actionOnClick;
        return element;
    }
}
