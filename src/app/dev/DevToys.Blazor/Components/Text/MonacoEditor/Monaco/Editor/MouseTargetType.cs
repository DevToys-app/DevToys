///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Type of hit element with the mouse in the editor.
/// </summary>
public enum MouseTargetType
{
    /// <summary>
    /// Mouse is on top of an unknown element.
    /// </summary>
    UNKNOWN = 0,

    /// <summary>
    /// Mouse is on top of the textarea used for input.
    /// </summary>
    TEXTAREA = 1,

    /// <summary>
    /// Mouse is on top of the glyph margin
    /// </summary>
    GUTTER_GLYPH_MARGIN = 2,

    /// <summary>
    /// Mouse is on top of the line numbers
    /// </summary>
    GUTTER_LINE_NUMBERS = 3,

    /// <summary>
    /// Mouse is on top of the line decorations
    /// </summary>
    GUTTER_LINE_DECORATIONS = 4,

    /// <summary>
    /// Mouse is on top of the whitespace left in the gutter by a view zone.
    /// </summary>
    GUTTER_VIEW_ZONE = 5,

    /// <summary>
    /// Mouse is on top of text in the content.
    /// </summary>
    CONTENT_TEXT = 6,

    /// <summary>
    /// Mouse is on top of empty space in the content (e.g. after line text or below last line)
    /// </summary>
    CONTENT_EMPTY = 7,

    /// <summary>
    /// Mouse is on top of a view zone in the content.
    /// </summary>
    CONTENT_VIEW_ZONE = 8,

    /// <summary>
    /// Mouse is on top of a content widget.
    /// </summary>
    CONTENT_WIDGET = 9,

    /// <summary>
    /// Mouse is on top of the decorations overview ruler.
    /// </summary>
    OVERVIEW_RULER = 10,

    /// <summary>
    /// Mouse is on top of a scrollbar.
    /// </summary>
    SCROLLBAR = 11,

    /// <summary>
    /// Mouse is on top of an overlay widget.
    /// </summary>
    OVERLAY_WIDGET = 12,

    /// <summary>
    /// Mouse is outside of the editor.
    /// </summary>
    OUTSIDE_EDITOR = 13
}
