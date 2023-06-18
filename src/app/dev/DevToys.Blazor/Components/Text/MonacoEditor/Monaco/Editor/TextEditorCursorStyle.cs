///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// The style in which the editor's cursor should be rendered.
/// </summary>
public enum TextEditorCursorStyle
{
    /// <summary>
    /// As a vertical line (sitting between two characters).
    /// </summary>
    Line = 1,

    /// <summary>
    /// As a block (sitting on top of a character).
    /// </summary>
    Block = 2,

    /// <summary>
    /// As a horizontal line (sitting under a character).
    /// </summary>
    Underline = 3,

    /// <summary>
    /// As a thin vertical line (sitting between two characters).
    /// </summary>
    LineThin = 4,

    /// <summary>
    /// As an outlined block (sitting on top of a character).
    /// </summary>
    BlockOutline = 5,

    /// <summary>
    /// As a thin horizontal line (sitting under a character).
    /// </summary>
    UnderlineThin = 6
}
