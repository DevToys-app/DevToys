#nullable enable

using DevToys.MonacoEditor.Monaco.Helpers;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Options for a model decoration.
    ///
    /// Options associated with this decoration.
    /// </summary>
    public sealed class ModelDecorationOptions
    {
        /// <summary>
        /// If set, the decoration will be rendered after the text with this CSS class name.
        /// </summary>
        [JsonProperty("afterContentClassName")]
        public string? AfterContentClassName { get; set; }

        /// <summary>
        /// If set, the decoration will be rendered before the text with this CSS class name.
        /// </summary>
        [JsonProperty("beforeContentClassName")]
        public string? BeforeContentClassName { get; set; }

        /// <summary>
        /// CSS class name describing the decoration.
        /// </summary>
        [JsonProperty("className")]
        public CssLineStyle? ClassName { get; set; }

        /// <summary>
        /// If set, the decoration will be rendered in the glyph margin with this CSS class name.
        /// </summary>
        [JsonProperty("glyphMarginClassName")]
        public CssGlyphStyle? GlyphMarginClassName { get; set; }

        /// <summary>
        /// Message to be rendered when hovering over the glyph margin decoration.
        /// </summary>
        [JsonProperty("glyphMarginHoverMessage")]
        public MarkdownString[]? GlyphMarginHoverMessage { get; set; }

        /// <summary>
        /// Array of MarkdownString to render as the decoration message.
        /// </summary>
        [JsonProperty("hoverMessage")]
        public MarkdownString[]? HoverMessage { get; set; }

        /// <summary>
        /// If set, the decoration will be rendered inline with the text with this CSS class name.
        /// Please use this only for CSS rules that must impact the text. For example, use
        /// `className`
        /// to have a background color decoration.
        /// </summary>
        [JsonProperty("inlineClassName")]
        public CssInlineStyle? InlineClassName { get; set; }

        /// <summary>
        /// If there is an `inlineClassName` which affects letter spacing.
        /// </summary>
        [JsonProperty("inlineClassNameAffectsLetterSpacing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InlineClassNameAffectsLetterSpacing { get; set; }

        /// <summary>
        /// Should the decoration expand to encompass a whole line.
        /// </summary>
        [JsonProperty("isWholeLine", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsWholeLine { get; set; }

        /// <summary>
        /// If set, the decoration will be rendered in the lines decorations with this CSS class name.
        /// </summary>
        [JsonProperty("linesDecorationsClassName")]
        public string? LinesDecorationsClassName { get; set; }

        /// <summary>
        /// If set, the decoration will be rendered in the margin (covering its full width) with this
        /// CSS class name.
        /// </summary>
        [JsonProperty("marginClassName")]
        public string? MarginClassName { get; set; }

        /// <summary>
        /// If set, render this decoration in the minimap.
        /// </summary>
        [JsonProperty("minimap")]
        public ModelDecorationMinimapOptions? Minimap { get; set; }

        /// <summary>
        /// If set, render this decoration in the overview ruler.
        /// </summary>
        [JsonProperty("overviewRuler")]
        public ModelDecorationOverviewRulerOptions? OverviewRuler { get; set; }

        /// <summary>
        /// Customize the growing behavior of the decoration when typing at the edges of the
        /// decoration.
        /// Defaults to TrackedRangeStickiness.AlwaysGrowsWhenTypingAtEdges
        /// </summary>
        [JsonProperty("stickiness", NullValueHandling = NullValueHandling.Ignore)]
        public TrackedRangeStickiness? Stickiness { get; set; }

        /// <summary>
        /// Specifies the stack order of a decoration.
        /// A decoration with greater stack order is always in front of a decoration with a lower
        /// stack order.
        /// </summary>
        [JsonProperty("zIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int? ZIndex { get; set; }
    }
}
