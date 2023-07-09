///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// The public layout details of the editor.
/// </summary>
public class EditorLayoutInfo
{
    /// <summary>
    /// Full editor width.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Full editor height.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Left position for the glyph margin.
    /// </summary>
    public float GlyphMarginLeft { get; set; }

    /// <summary>
    /// The width of the glyph margin.
    /// </summary>
    public float GlyphMarginWidth { get; set; }

    /// <summary>
    /// Left position for the line numbers.
    /// </summary>
    public float LineNumbersLeft { get; set; }

    /// <summary>
    /// The width of the line numbers.
    /// </summary>
    public float LineNumbersWidth { get; set; }

    /// <summary>
    /// Left position for the line decorations.
    /// </summary>
    public float DecorationsLeft { get; set; }

    /// <summary>
    /// The width of the line decorations.
    /// </summary>
    public float DecorationsWidth { get; set; }

    /// <summary>
    /// Left position for the content (actual text)
    /// </summary>
    public float ContentLeft { get; set; }

    /// <summary>
    /// The width of the content (actual text)
    /// </summary>
    public float ContentWidth { get; set; }

    /// <summary>
    /// Layout information for the minimap
    /// </summary>
    public EditorMinimapLayoutInfo? Minimap { get; set; }

    /// <summary>
    /// The number of columns (of typical characters) fitting on a viewport line.
    /// </summary>
    public float ViewportColumn { get; set; }

    public bool IsWordWrapMinified { get; set; }

    public bool IsViewportWrapping { get; set; }

    public float WrappingColumn { get; set; }

    /// <summary>
    /// The width of the vertical scrollbar.
    /// </summary>
    public float VerticalScrollbarWidth { get; set; }

    /// <summary>
    /// The height of the horizontal scrollbar.
    /// </summary>
    public float HorizontalScrollbarHeight { get; set; }

    /// <summary>
    /// The position of the overview ruler.
    /// </summary>
    public OverviewRulerPosition? OverviewRuler { get; set; }
}
