namespace DevToys.Api;

/// <summary>
/// A component that represents a menu item, which reacts when clicking on it.
/// </summary>
public interface IUIUtf32IconDropDownMenuItem : IUIDropDownMenuItem, IUIUtf32IconProvider
{
}

[DebuggerDisplay($"Text = {{{nameof(Text)}}}")]
internal sealed class UIUtf32IconDropDownMenuItem : UIDropDownMenuItem, IUIUtf32IconDropDownMenuItem
{
    private int _utf32IconGlyph;
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
    /// Create a component that represents a menu item, which reacts when clicking on it.
    /// </summary>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    public static IUIUtf32IconDropDownMenuItem DropDownMenuItem(string iconFontName, int iconGlyph)
    {
        return new UIUtf32IconDropDownMenuItem().Icon(iconFontName, iconGlyph);
    }

    /// <summary>
    /// Create a component that represents a menu item, which reacts when clicking on it.
    /// </summary>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    /// <param name="text">The text to display in the menu item.</param>
    public static IUIUtf32IconDropDownMenuItem DropDownMenuItem(string iconFontName, int iconGlyph, string text)
    {
        return (IUIUtf32IconDropDownMenuItem)DropDownMenuItem(iconFontName, iconGlyph).Text(text);
    }

    /// <summary>
    /// Sets the icon of the menu item.
    /// </summary>
    public static IUIUtf32IconDropDownMenuItem Icon(this IUIUtf32IconDropDownMenuItem element, string fontName, int glyph)
    {
        var menuItem = (UIUtf32IconDropDownMenuItem)element;
        menuItem.IconFontName = fontName;
        menuItem.Utf32IconGlyph = glyph;
        return menuItem;
    }
}
