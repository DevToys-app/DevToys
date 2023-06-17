///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

/// <summary>
/// A selection in the editor.
/// The selection is a range that has an orientation.
/// </summary>
public class Selection : Range
{
    /// <summary>
    /// The line number on which the selection has started.
    /// </summary>
    public int SelectionStartLineNumber { get; set; }

    /// <summary>
    /// The column on `selectionStartLineNumber` where the selection has started.
    /// </summary>
    public int SelectionStartColumn { get; set; }

    /// <summary>
    /// The line number on which the selection has ended.
    /// </summary>
    public int PositionLineNumber { get; set; }

    /// <summary>
    /// The column on `positionLineNumber` where the selection has ended.
    /// </summary>
    public int PositionColumn { get; set; }
}
