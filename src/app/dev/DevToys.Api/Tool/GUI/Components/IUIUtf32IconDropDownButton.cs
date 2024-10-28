namespace DevToys.Api;

/// <summary>
/// A component that represents a drop down button where the user can click on a menu item.
/// </summary>
public interface IUIUtf32IconDropDownButton : IUIDropDownButton, IUIUtf32IconProvider
{
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UIUtf32IconDropDownButton : UIDropDownButton, IUIUtf32IconDropDownButton
{
    private int _utf32IconGlyph;
    internal UIUtf32IconDropDownButton(string? id)
        : base(id)
    {
    }

    public int Utf32IconGlyph
    {
        get => _utf32IconGlyph;
        internal set => SetPropertyValue(ref _utf32IconGlyph, value, Utf32IconGlyphChanged);
    }

    public event EventHandler? Utf32IconGlyphChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    public static IUIUtf32IconDropDownButton DropDownButton(string iconFontName, int iconGlyph)
    {
        return DropDownButton(null, iconFontName, iconGlyph);
    }

    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    public static IUIUtf32IconDropDownButton DropDownButton(string? id, string iconFontName, int iconGlyph)
    {
        return new UIUtf32IconDropDownButton(id).Icon(iconFontName, iconGlyph);
    }

    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    /// <param name="text">The text to display in the drop down button.</param>
    public static IUIUtf32IconDropDownButton DropDownButton(string? id, string iconFontName, int iconGlyph, string text)
    {
        return (IUIUtf32IconDropDownButton)new UIUtf32IconDropDownButton(id).Icon(iconFontName, iconGlyph).Text(text);
    }

    /// <summary>
    /// Create a component that represents a drop down button where the user can click on a menu item.
    /// </summary>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    /// <param name="text">The text to display in the drop down button.</param>
    public static IUIUtf32IconDropDownButton DropDownButton(string iconFontName, int iconGlyph, string text)
    {
        return DropDownButton(null, iconFontName, iconGlyph, text);
    }

    /// <summary>
    /// Sets the icon of the drop down button.
    /// </summary>
    public static IUIUtf32IconDropDownButton Icon(this IUIUtf32IconDropDownButton element, string fontName, int glyph)
    {
        var button = (UIUtf32IconDropDownButton)element;
        button.IconFontName = fontName;
        button.Utf32IconGlyph = glyph;
        return button;
    }
}
