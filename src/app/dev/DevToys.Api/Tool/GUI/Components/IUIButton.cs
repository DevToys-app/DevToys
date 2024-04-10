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
    /// Gets whether the button appearance should be a hyperlink.
    /// </summary>
    bool IsHyperlink { get; }

    /// <summary>
    /// Gets the text to display in the button.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets the name of the font containing the icon.
    /// </summary>
    string? IconFontName { get; }

    /// <summary>
    /// Gets the glyph corresponding to the icon in the <see cref="IconFontName"/>.
    /// </summary>
    char IconGlyph { get; }

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

    /// <summary>
    /// Raised when <see cref="IconFontName"/> is changed.
    /// </summary>
    event EventHandler? IconFontNameChanged;

    /// <summary>
    /// Raised when <see cref="IconGlyph"/> is changed.
    /// </summary>
    event EventHandler? IconGlyphChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UIButton : UIElement, IUIButton
{
    private bool _isAccent;
    private bool _isHyperlink;
    private string? _text;
    private string? _iconFontName;
    private char _iconGlyph;

    internal UIButton(string? id)
        : base(id)
    {
    }

    public bool IsAccent
    {
        get => _isAccent;
        internal set
        {
            if (_isAccent != value)
            {
                SetPropertyValue(ref _isAccent, value, IsAccentChanged);
                IsHyperlink = false;
            }
        }
    }

    public bool IsHyperlink
    {
        get => _isHyperlink;
        internal set
        {
            if (_isHyperlink != value)
            {
                SetPropertyValue(ref _isHyperlink, value, IsAccentChanged);
                IsAccent = false;
            }
        }
    }

    public string? Text
    {
        get => _text;
        internal set => SetPropertyValue(ref _text, value, TextChanged);
    }

    public string? IconFontName
    {
        get => _iconFontName;
        internal set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            SetPropertyValue(ref _iconFontName, value, IconFontNameChanged);
        }
    }

    public char IconGlyph
    {
        get => _iconGlyph;
        internal set => SetPropertyValue(ref _iconGlyph, value, IconGlyphChanged);
    }

    public Func<ValueTask>? OnClickAction { get; internal set; }

    public event EventHandler? TextChanged;
    public event EventHandler? IsAccentChanged;
    public event EventHandler? IconFontNameChanged;
    public event EventHandler? IconGlyphChanged;
}

/// <summary>
/// Provides a set of extension methods for various UI components.
/// </summary>
public static partial class GUI
{
    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <returns>The created <see cref="IUIButton"/> instance.</returns>
    public static IUIButton Button()
    {
        return Button(null);
    }

    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <returns>The created <see cref="IUIButton"/> instance.</returns>
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
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <param name="text">The text to display in the button.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton Text(this IUIButton element, string? text)
    {
        ((UIButton)element).Text = text;
        return element;
    }

    /// <summary>
    /// Sets the action to run when clicking on the button.
    /// </summary>
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <param name="actionOnClick">The action to run when clicking on the button.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton OnClick(this IUIButton element, Func<ValueTask>? actionOnClick)
    {
        ((UIButton)element).OnClickAction = actionOnClick;
        return element;
    }

    /// <summary>
    /// Sets the action to run when clicking on the button.
    /// </summary>
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <param name="actionOnClick">The action to run when clicking on the button.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton OnClick(this IUIButton element, Action? actionOnClick)
    {
        ((UIButton)element).OnClickAction
            = () =>
            {
                actionOnClick?.Invoke();
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the button to appear as accented.
    /// </summary>
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton AccentAppearance(this IUIButton element)
    {
        ((UIButton)element).IsAccent = true;
        return element;
    }

    /// <summary>
    /// Sets the button to appear as a hyperlink.
    /// </summary>
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton HyperlinkAppearance(this IUIButton element)
    {
        ((UIButton)element).IsHyperlink = true;
        return element;
    }

    /// <summary>
    /// Sets the button to appear as neutral.
    /// </summary>
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton NeutralAppearance(this IUIButton element)
    {
        ((UIButton)element).IsAccent = false;
        return element;
    }

    /// <summary>
    /// Sets the icon of the button.
    /// </summary>
    /// <param name="element">The <see cref="IUIButton"/> instance.</param>
    /// <param name="fontName">The name of the font containing the icon.</param>
    /// <param name="glyph">The glyph corresponding to the icon in the <paramref name="fontName"/>.</param>
    /// <returns>The updated <see cref="IUIButton"/> instance.</returns>
    public static IUIButton Icon(this IUIButton element, string fontName, char glyph)
    {
        var button = (UIButton)element;
        button.IconFontName = fontName;
        button.IconGlyph = glyph;
        return button;
    }
}
