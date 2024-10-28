namespace DevToys.Api;

/// <summary>
/// A component that represents an icon with a UTF-32 glyph.
/// </summary>
public interface IUIUtf32Icon : IUIIcon, IUIUtf32GlyphProvider
{
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Glyph = {{{nameof(Glyph)}}}")]
internal sealed class UIUtf32Icon : UIIcon, IUIUtf32Icon
{
    private int _utf32Glyph;

    internal UIUtf32Icon(string? id, string fontName, int glyph)
        : base(id, fontName, (char)0)
    {
        _utf32Glyph = glyph;
        Guard.IsNotNullOrWhiteSpace(FontName);
    }

    public int Utf32Glyph
    {
        get => _utf32Glyph;
        internal set => SetPropertyValue(ref _utf32Glyph, value, Glyph32Changed);
    }

    public event EventHandler? Glyph32Changed;
}

public static partial class GUI
{

    /// <summary>
    /// A component that represents an icon with a UTF-32 glyph.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="fontName">The name of the font containing the icon.</param>
    /// <param name="glyph">The UTF-32 code point of the icon glyph.</param>
    public static IUIUtf32Icon Icon(string? id, string fontName, int glyph)
    {
        return new UIUtf32Icon(id, fontName, glyph);
    }

    /// <summary>
    /// A component that represents an icon with a UTF-32 glyph.
    /// </summary>
    /// <param name="fontName">The name of the font containing the icon.</param>
    /// <param name="glyph">The UTF-32 code point of the icon glyph.</param>
    public static IUIUtf32Icon Icon(string fontName, int glyph)
    {
        return new UIUtf32Icon(null, fontName, glyph);
    }

    /// <summary>
    /// Sets the <see cref="UIUtf32Icon.Utf32Glyph"/> of the icon.
    /// </summary>
    /// <param name="element">The <see cref="UIUtf32Icon"/> instance.</param>
    /// <param name="glyph">The UTF-32 code point of the icon glyph.</param>
    public static IUIUtf32Icon Glyph(this IUIUtf32Icon element, int glyph)
    {
        ((UIUtf32Icon)element).Utf32Glyph = glyph;
        return element;
    }
}
