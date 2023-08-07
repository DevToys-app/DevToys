namespace DevToys.Api;

/// <summary>
/// A component that displays a text.
/// </summary>
public interface IUILabel : IUIElement
{
    /// <summary>
    /// Gets the text to display.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets the style of the text.
    /// Default value is <see cref="UILabelStyle.Body"/>.
    /// </summary>
    UILabelStyle Style { get; }

    /// <summary>
    /// Gets whether the text can wrap if it can't fit within the available horizontal space.
    /// Default is true.
    /// </summary>
    bool AutoWrap { get; }

    /// <summary>
    /// Raised when <see cref="Text"/> is changed.
    /// </summary>
    event EventHandler? TextChanged;

    /// <summary>
    /// Raised when <see cref="Style"/> is changed.
    /// </summary>
    event EventHandler? StyleChanged;

    /// <summary>
    /// Raised when <see cref="AutoWrap"/> is changed.
    /// </summary>
    event EventHandler? AutoWrapChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UILabel : UIElement, IUILabel
{
    private UILabelStyle _style = UILabelStyle.Body;
    private string? _text;
    private bool _autoWrap = true;

    internal UILabel(string? id)
        : base(id)
    {
    }

    public string? Text
    {
        get => _text;
        internal set => SetPropertyValue(ref _text, value, TextChanged);
    }

    public UILabelStyle Style
    {
        get => _style;
        internal set => SetPropertyValue(ref _style, value, StyleChanged);
    }

    public bool AutoWrap
    {
        get => _autoWrap;
        internal set => SetPropertyValue(ref _autoWrap, value, AutoWrapChanged);
    }

    public event EventHandler? TextChanged;

    public event EventHandler? StyleChanged;

    public event EventHandler? AutoWrapChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that displays a text.
    /// </summary>
    public static IUILabel Label()
    {
        return Label(null);
    }

    /// <summary>
    /// A component that displays a text.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUILabel Label(string? id)
    {
        return new UILabel(id);
    }

    /// <summary>
    /// A component that displays a text.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="text">The text to display.</param>
    public static IUILabel Label(string? id, string text)
    {
        return new UILabel(id).Text(text);
    }

    /// <summary>
    /// Sets the <see cref="IUILabel.Text"/>.
    /// </summary>
    public static IUILabel Text(this IUILabel element, string? text)
    {
        ((UILabel)element).Text = text;
        return element;
    }

    /// <summary>
    /// Sets the text style.
    /// </summary>
    public static IUILabel Style(this IUILabel element, UILabelStyle style)
    {
        ((UILabel)element).Style = style;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUILabel.AutoWrap"/> to true.
    /// </summary>
    public static IUILabel WrapIfNeeded(this IUILabel element)
    {
        ((UILabel)element).AutoWrap = true;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUILabel.AutoWrap"/> to false.
    /// </summary>
    public static IUILabel NeverWrap(this IUILabel element)
    {
        ((UILabel)element).AutoWrap = false;
        return element;
    }
}
