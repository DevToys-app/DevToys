namespace DevToys.Api;

/// <summary>
/// A component that represents a button, which reacts when clicking on it.
/// </summary>
public interface IUIButton : IUIElement
{
    /// <summary>
    /// Gets the text to display in the button.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets the action to run when the user clicks the button.
    /// </summary>
    Func<ValueTask>? OnClickAction { get; }

    /// <summary>
    /// Raised when <see cref="Text"/> is changed.
    /// </summary>
    public event EventHandler? TextChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UIButton : UIElement, IUIButton
{
    private string? _text;

    internal UIButton(string? id)
        : base(id)
    {
    }

    public string? Text
    {
        get => _text;
        internal set
        {
            _text = value;
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Func<ValueTask>? OnClickAction { get; internal set; }

    public event EventHandler? TextChanged;
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
    /// Sets the <see cref="IUIButton.Text"/> of the button.
    /// </summary>
    public static IUIButton Text(this IUIButton element, string? text)
    {
        ((UIButton)element).Text = text;
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
