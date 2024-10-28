namespace DevToys.Api;

/// <summary>
/// Provides an interface for UI components that support UTF-32 glyphs.
/// </summary>
public interface IUIUtf32GlyphProvider
{
    /// <summary>
    /// Gets the UTF-32 code point of the glyph.
    /// </summary>
    int Utf32Glyph { get; }
}
