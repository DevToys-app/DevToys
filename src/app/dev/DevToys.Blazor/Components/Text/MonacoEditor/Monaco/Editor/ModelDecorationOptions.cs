///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Options for a model decoration.
/// </summary>
public class ModelDecorationOptions
{
    /// <summary>
    /// Customize the growing behavior of the decoration when typing at the edges of the decoration.
    /// Defaults to TrackedRangeStickiness.AlwaysGrowsWhenTypingAtEdges
    /// </summary>
    public TrackedRangeStickiness? Stickiness { get; set; }

    /// <summary>
    /// CSS class name describing the decoration.
    /// </summary>
    public string? ClassName { get; set; }

    public string? BlockClassName { get; set; }

    /// <summary>
    /// Message to be rendered when hovering over the glyph margin decoration.
    /// </summary>
    public MarkdownString[]? GlyphMarginHoverMessage { get; set; }

    /// <summary>
    /// Array of MarkdownString to render as the decoration message.
    /// </summary>
    public MarkdownString[]? HoverMessage { get; set; }

    /// <summary>
    /// Should the decoration expand to encompass a whole line.
    /// </summary>
    public bool? IsWholeLine { get; set; }

    /// <summary>
    /// Always render the decoration (even when the range it encompasses is collapsed).
    /// </summary>
    public bool? ShowIfCollapsed { get; set; }

    /// <summary>
    /// Specifies the stack order of a decoration.
    /// A decoration with greater stack order is always in front of a decoration with
    /// a lower stack order when the decorations are on the same line.
    /// </summary>
    public int? ZIndex { get; set; }

    /// <summary>
    /// If set, render this decoration in the overview ruler.
    /// </summary>
    public ModelDecorationOverviewRulerOptions? OverviewRuler { get; set; }

    /// <summary>
    /// If set, render this decoration in the minimap.
    /// </summary>
    public ModelDecorationMinimapOptions? Minimap { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered in the glyph margin with this CSS class name.
    /// </summary>
    public string? GlyphMarginClassName { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered in the lines decorations with this CSS class name.
    /// </summary>
    public string? LinesDecorationsClassName { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered in the lines decorations with this CSS class name, but only for the first line in case of line wrapping.
    /// </summary>
    public string? FirstLineDecorationClassName { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered in the margin (covering its full width) with this CSS class name.
    /// </summary>
    public string? MarginClassName { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered inline with the text with this CSS class name.
    /// Please use this only for CSS rules that must impact the text. For example, use `className`
    /// to have a background color decoration.
    /// </summary>
    public string? InlineClassName { get; set; }

    /// <summary>
    /// If there is an `inlineClassName` which affects letter spacing.
    /// </summary>
    public bool? InlineClassNameAffectsLetterSpacing { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered before the text with this CSS class name.
    /// </summary>
    public string? BeforeContentClassName { get; set; }

    /// <summary>
    /// If set, the decoration will be rendered after the text with this CSS class name.
    /// </summary>
    public string? AfterContentClassName { get; set; }

    /// <summary>
    /// If set, text will be injected in the view after the range.
    /// </summary>
    public InjectedTextOptions? After { get; set; }

    /// <summary>
    /// If set, text will be injected in the view before the range.
    /// </summary>
    public InjectedTextOptions? Before { get; set; }
}
