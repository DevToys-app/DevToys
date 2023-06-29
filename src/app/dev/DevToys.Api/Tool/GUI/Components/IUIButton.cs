namespace DevToys.Api;

/// <summary>
/// A component that represents a button, which reacts when clicking on it.
/// </summary>
public interface IUIButton : IUIElement
{
    /// <summary>
    /// Gets whether the button appearance should be accented.
    /// </summary>
    bool IsAccent { get; }

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
    event EventHandler? TextChanged;

    /// <summary>
    /// Raised when <see cref="IsAccent"/> is changed.
    /// </summary>
    event EventHandler? IsAccentChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UIButton : UIElement, IUIButton
{
    private bool _isAccent;
    private string? _text;

    internal UIButton(string? id)
        : base(id)
    {
    }

    public bool IsAccent
    {
        get => _isAccent;
        internal set => SetPropertyValue(ref _isAccent, value, IsAccentChanged);
    }

    public string? Text
    {
        get => _text;
        internal set => SetPropertyValue(ref _text, value, TextChanged);
    }

    public Func<ValueTask>? OnClickAction { get; internal set; }

    public event EventHandler? TextChanged;

    public event EventHandler? IsAccentChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    public static IUIButton Button()
    {
        return Button(null);
    }

    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIButton Button(string? id)
    {
        return new UIButton(id);
    }

    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="text">The text to display in the button.</param>
    public static IUIButton Button(string? id, string text)
    {
        return new UIButton(id).Text(text);
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

    /// <summary>
    /// Sets the button to appear as accented.
    /// </summary>
    public static IUIButton AccentAppearance(this IUIButton element)
    {
        ((UIButton)element).IsAccent = true;
        return element;
    }

    /// <summary>
    /// Sets the button to appear as neutral.
    /// </summary>
    public static IUIButton NeutralAppearance(this IUIButton element)
    {
        ((UIButton)element).IsAccent = false;
        return element;
    }
}
