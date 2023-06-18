///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class ValidEditOperation
{
    /// <summary>
    /// The range to replace. This can be empty to emulate a simple insert.
    /// </summary>
    public Range? Range { get; set; }

    /// <summary>
    /// The text to replace with. This can be empty to emulate a simple delete.
    /// </summary>
    public string? Text { get; set; }
}
