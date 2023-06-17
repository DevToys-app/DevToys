///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing that the cursor selection has changed.
/// </summary>
public class CursorSelectionChangedEvent
{
    /// <summary>
    /// The primary selection.
    /// </summary>
    public Selection? Selection { get; set; }

    /// <summary>
    /// The secondary selections.
    /// </summary>
    public List<Selection>? SecondarySelections { get; set; }

    /// <summary>
    /// The model version id.
    /// </summary>
    public int ModelVersionId { get; set; }

    /// <summary>
    /// The old selections.
    /// </summary>
    public List<Selection>? OldSelections { get; set; }

    /// <summary>
    /// The model version id the that `oldSelections` refer to.
    /// </summary>
    public int OldModelVersionId { get; set; }

    /// <summary>
    /// Source of the call that caused the event.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Reason.
    /// </summary>
    public CursorChangeReason Reason { get; set; }
}
