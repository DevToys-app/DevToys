namespace DevToys.Api;

/// <summary>
/// A component that represents a menu item, which reacts when clicking on it.
/// </summary>
public interface IUIDropDownMenuItem
{
    /// <summary>
    /// Gets whether this menu item should be enabled or disabled. Default is true.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the text to display in the menu item.
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
    /// Gets the action to run when the user clicks the menu item.
    /// </summary>
    Func<ValueTask>? OnClickAction { get; }

    /// <summary>
    /// Raised when <see cref="IsEnabled"/> is changed.
    /// </summary>
    event EventHandler? IsEnabledChanged;

    /// <summary>
    /// Raised when <see cref="Text"/> is changed.
    /// </summary>
    event EventHandler? TextChanged;

    /// <summary>
    /// Raised when <see cref="IconFontName"/> is changed.
    /// </summary>
    event EventHandler? IconFontNameChanged;

    /// <summary>
    /// Raised when <see cref="IconGlyph"/> is changed.
    /// </summary>
    event EventHandler? IconGlyphChanged;
}

[DebuggerDisplay($"Text = {{{nameof(Text)}}}")]
internal sealed class UIDropDownMenuItem : ViewModelBase, IUIDropDownMenuItem
{
    private bool _isEnabled = true;
    private string? _text;
    private string? _iconFontName;
    private char _iconGlyph;

    public bool IsEnabled
    {
        get => _isEnabled;
        internal set
        {
            if (_isEnabled != value)
            {
                SetPropertyValue(ref _isEnabled, value, IsEnabledChanged);
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

    public event EventHandler? IsEnabledChanged;
    public event EventHandler? TextChanged;
    public event EventHandler? IconFontNameChanged;
    public event EventHandler? IconGlyphChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that represents a menu item, which reacts when clicking on it.
    /// </summary>
    public static IUIDropDownMenuItem DropDownMenuItem()
    {
        return new UIDropDownMenuItem();
    }

    /// <summary>
    /// Create a component that represents a menu item, which reacts when clicking on it.
    /// </summary>
    /// <param name="text">The text to display in the menu item.</param>
    public static IUIDropDownMenuItem DropDownMenuItem(string text)
    {
        return DropDownMenuItem().Text(text);
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownMenuItem.Text"/> of the menu item.
    /// </summary>
    public static IUIDropDownMenuItem Text(this IUIDropDownMenuItem element, string? text)
    {
        ((UIDropDownMenuItem)element).Text = text;
        return element;
    }

    /// <summary>
    /// Sets the action to run when clicking on the menu item.
    /// </summary>
    public static IUIDropDownMenuItem OnClick(this IUIDropDownMenuItem element, Func<ValueTask>? actionOnClick)
    {
        ((UIDropDownMenuItem)element).OnClickAction = actionOnClick;
        return element;
    }

    /// <summary>
    /// Sets the action to run when clicking on the menu item.
    /// </summary>
    public static IUIDropDownMenuItem OnClick(this IUIDropDownMenuItem element, Action? actionOnClick)
    {
        ((UIDropDownMenuItem)element).OnClickAction
            = () =>
            {
                actionOnClick?.Invoke();
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the icon of the menu item.
    /// </summary>
    public static IUIDropDownMenuItem Icon(this IUIDropDownMenuItem element, string fontName, char glyph)
    {
        var menuItem = (UIDropDownMenuItem)element;
        menuItem.IconFontName = fontName;
        menuItem.IconGlyph = glyph;
        return menuItem;
    }
}
