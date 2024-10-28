namespace DevToys.Api;

/// <summary>
/// Provides an interface for UI components that support UTF-32 icons.
/// </summary>
public interface IUIUtf32IconProvider
{
    /// <summary>
    /// Gets the UTF-32 code point of the icon glyph.
    /// </summary>
    int Utf32IconGlyph { get; }
}
