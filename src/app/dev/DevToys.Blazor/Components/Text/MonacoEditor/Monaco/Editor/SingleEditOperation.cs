///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A single edit operation, that acts as a simple replace.
/// i.e. Replace text at `range` with `text` in model.
/// </summary>
public class SingleEditOperation
{
    /// <summary>
    /// The range to replace. This can be empty to emulate a simple insert.
    /// </summary>
    public Range? Range { get; set; }

    /// <summary>
    /// The text to replace with. This can be null to emulate a simple delete.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// This indicates that this operation has "insert" semantics.
    /// i.e. forceMoveMarkers = true => if `range` is collapsed, all markers at the position will be moved.
    /// </summary>
    public bool? ForceMoveMarkers { get; set; }
}
