///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A model for the diff editor.
/// </summary>
public class DiffEditorModel
{
    /// <summary>
    /// Original model.
    /// </summary>
    public TextModel? Original { get; set; }

    /// <summary>
    /// Modified model.
    /// </summary>
    public TextModel? Modified { get; set; }
}
