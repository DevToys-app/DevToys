namespace DevToys.Api;

/// <summary>
/// A component that represents a drop down button where the user can click on a menu item.
/// </summary>
public interface IUIDropDownButton : IUIElement
{
    /// <summary>
    /// Gets the list of items displayed in the drop down menu.
    /// </summary>
    IUIDropDownMenuItem[]? MenuItems { get; }

    /// <summary>
    /// Gets the text to display in the drop down button.
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

    ///<summary>
    /// Raised when <see cref="MenuItems"/> is changed.
    /// </summary>
    event EventHandler? MenuItemsChanged;

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

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UIDropDownButton : UIElement, IUIDropDownButton
{
    private IUIDropDownMenuItem[]? _menuItems;
    private string? _text;
    private string? _iconFontName;
    private char _iconGlyph;

    internal UIDropDownButton(string? id)
        : base(id)
    {
    }

    public IUIDropDownMenuItem[]? MenuItems
    {
        get => _menuItems;
        internal set => SetPropertyValue(ref _menuItems, value, MenuItemsChanged);
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

    public event EventHandler? MenuItemsChanged;
    public event EventHandler? TextChanged;
    public event EventHandler? IconFontNameChanged;
    public event EventHandler? IconGlyphChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    public static IUIDropDownButton DropDownButton()
    {
        return DropDownButton(null);
    }

    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIDropDownButton DropDownButton(string? id)
    {
        return new UIDropDownButton(id);
    }

    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="text">The text to display in the drop down button.</param>
    public static IUIDropDownButton DropDownButton(string? id, string text)
    {
        return new UIDropDownButton(id).Text(text);
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownButton.Text"/> of the drop down button.
    /// </summary>
    public static IUIDropDownButton Text(this IUIDropDownButton element, string? text)
    {
        ((UIDropDownButton)element).Text = text;
        return element;
    }

    /// <summary>
    /// Sets the icon of the drop down button.
    /// </summary>
    public static IUIDropDownButton Icon(this IUIDropDownButton element, string fontName, char glyph)
    {
        var button = (UIDropDownButton)element;
        button.IconFontName = fontName;
        button.IconGlyph = glyph;
        return button;
    }

    /// <summary>
    /// Sets the <see cref="IUIDropDownButton.MenuItems"/> in the drop down button.
    /// </summary>
    public static IUIDropDownButton WithMenuItems(this IUIDropDownButton element, params IUIDropDownMenuItem[] menuItems)
    {
        ((UIDropDownButton)element).MenuItems = menuItems;
        return element;
    }
}
