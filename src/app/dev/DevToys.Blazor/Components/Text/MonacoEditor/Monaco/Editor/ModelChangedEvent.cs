///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing that an editor has had its model reset (i.e. `editor.setModel()`).
/// </summary>
public class ModelChangedEvent
{
    /// <summary>
    /// The `uri` of the previous model or null.
    /// </summary>
    public string? OldModelUri { get; set; }

    /// <summary>
    /// The `uri` of the new model or null.
    /// </summary>
    public string? NewModelUri { get; set; }
}
