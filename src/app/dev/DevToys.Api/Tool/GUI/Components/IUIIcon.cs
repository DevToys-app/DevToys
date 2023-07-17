namespace DevToys.Api;

/// <summary>
/// A component that represents an icon.
/// </summary>
public interface IUIIcon : IUIElement
{
    /// <summary>
    /// Gets the name of the font containing the icon.
    /// </summary>
    string FontName { get; }

    /// <summary>
    /// Gets the glyph corresponding to the icon in the <see cref="FontName"/>.
    /// </summary>
    char Glyph { get; }

    /// <summary>
    /// Gets the size of the icon.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Raised when <see cref="FontName"/> is changed.
    /// </summary>
    event EventHandler? FontNameChanged;

    /// <summary>
    /// Raised when <see cref="Glyph"/> is changed.
    /// </summary>
    event EventHandler? GlyphChanged;

    /// <summary>
    /// Raised when <see cref="Size"/> is changed.
    /// </summary>
    event EventHandler? SizeChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Glyph = {{{nameof(Glyph)}}}")]
internal sealed class UIIcon : UIElement, IUIIcon
{
    private string _fontName;
    private char _glyph;
    private int _size = 16;

    internal UIIcon(string? id, string fontName, char glyph)
        : base(id)
    {
        _fontName = fontName;
        _glyph = glyph;
        Guard.IsNotNullOrWhiteSpace(FontName);
    }

    public string FontName
    {
        get => _fontName;
        internal set
        {
            Guard.IsNotNullOrWhiteSpace(value);
            SetPropertyValue(ref _fontName, value, FontNameChanged);
        }
    }

    public char Glyph
    {
        get => _glyph;
        internal set => SetPropertyValue(ref _glyph, value, GlyphChanged);
    }

    public int Size
    {
        get => _size;
        internal set => SetPropertyValue(ref _size, value, SizeChanged);
    }

    public event EventHandler? FontNameChanged;
    public event EventHandler? GlyphChanged;
    public event EventHandler? SizeChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents an icon.
    /// </summary>
    /// <param name="fontName">The name of the font containing the icon.</param>
    /// <param name="glyph">The glyph corresponding to the icon</param>
    public static IUIIcon Icon(string fontName, char glyph)
    {
        return Icon(id: null, fontName, glyph);
    }

    /// <summary>
    /// A component that represents an icon.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="fontName">The name of the font containing the icon.</param>
    /// <param name="glyph">The glyph corresponding to the icon</param>
    public static IUIIcon Icon(string? id, string fontName, char glyph)
    {
        return new UIIcon(id, fontName, glyph);
    }

    /// <summary>
    /// Sets the <see cref="IUIIcon.FontName"/> of the icon.
    /// </summary>
    public static IUIIcon FontName(this IUIIcon element, string fontName)
    {
        ((UIIcon)element).FontName = fontName;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIIcon.Glyph"/> of the icon.
    /// </summary>
    public static IUIIcon Glyph(this IUIIcon element, char glyph)
    {
        ((UIIcon)element).Glyph = glyph;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIIcon.Size"/> of the icon.
    /// </summary>
    public static IUIIcon Size(this IUIIcon element, int size)
    {
        ((UIIcon)element).Size = size;
        return element;
    }
}
