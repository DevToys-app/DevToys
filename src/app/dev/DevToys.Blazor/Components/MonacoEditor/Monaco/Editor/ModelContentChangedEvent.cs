///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing a change in the text of a model.
/// </summary>
public class ModelContentChangedEvent
{
    public List<ModelContentChange>? Changes { get; set; }

    /// <summary>
    /// The (new) end-of-line character.
    /// </summary>
    public string? Eol { get; set; }

    /// <summary>
    /// The new version id the model has transitioned to.
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Flag that indicates that this event was generated while undoing.
    /// </summary>
    public bool IsUndoing { get; set; }

    /// <summary>
    /// Flag that indicates that this event was generated while redoing.
    /// </summary>
    public bool IsRedoing { get; set; }

    /// <summary>
    /// Flag that indicates that all decorations were lost with this edit.
    /// The model has been reset to a new value.
    /// </summary>
    public bool IsFlush { get; set; }
}
