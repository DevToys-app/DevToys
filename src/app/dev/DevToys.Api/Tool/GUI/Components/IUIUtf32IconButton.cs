namespace DevToys.Api;

/// <summary>
/// A component that represents a button, which reacts when clicking on it.
/// </summary>
public interface IUIUtf32IconButton : IUIButton, IUIUtf32IconProvider
{
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal sealed class UIUtf32IconButton : UIButton, IUIUtf32IconButton
{
    private int _iconGlyph;

    internal UIUtf32IconButton(string? id)
        : base(id)
    {
    }

    public int Utf32IconGlyph
    {
        get => _iconGlyph;
        internal set => SetPropertyValue(ref _iconGlyph, value, Utf32IconGlyphChanged);
    }
    public event EventHandler? Utf32IconGlyphChanged;
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
    public static IUIUtf32IconButton Button(string iconFontName, int iconGlyph)
    {
        return Button(null, iconFontName, iconGlyph);
    }

    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    /// <returns>The created <see cref="IUIUtf32IconButton"/> instance.</returns>
    public static IUIUtf32IconButton Button(string? id, string iconFontName, int iconGlyph)
    {
        return new UIUtf32IconButton(id).Icon(iconFontName, iconGlyph);
    }

    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    /// <param name="text">The text to display in the button.</param>
    /// <returns>The created <see cref="IUIUtf32IconButton"/> instance.</returns>
    public static IUIUtf32IconButton Button(string? id, string iconFontName, int iconGlyph, string text)
    {
        return (IUIUtf32IconButton)new UIUtf32IconButton(id).Icon(iconFontName, iconGlyph).Text(text);
    }

    /// <summary>
    /// Create a component that represents a button, which reacts when clicking on it.
    /// </summary>
    /// <param name="iconFontName">The name of the font containing the icon.</param>
    /// <param name="iconGlyph">The UTF-32 glyph corresponding to the icon in the <paramref name="iconFontName"/>.</param>
    /// <param name="text">The text to display in the button.</param>
    /// <returns>The created <see cref="IUIUtf32IconButton"/> instance.</returns>
    public static IUIUtf32IconButton Button(string iconFontName, int iconGlyph, string text)
    {
        return Button(null, iconFontName, iconGlyph, text);
    }

    /// <summary>
    /// Sets the icon of the button.
    /// </summary>
    /// <param name="element">The <see cref="IUIUtf32IconButton"/> instance.</param>
    /// <param name="fontName">The name of the font containing the icon.</param>
    /// <param name="glyph">The UTF-32 glyph corresponding to the icon in the <paramref name="fontName"/>.</param>
    /// <returns>The updated <see cref="IUIUtf32IconButton"/> instance.</returns>
    public static IUIUtf32IconButton Icon(this IUIUtf32IconButton element, string fontName, int glyph)
    {
        var button = (UIUtf32IconButton)element;
        button.IconFontName = fontName;
        button.Utf32IconGlyph = glyph;
        return button;
    }
}
