///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// A paste event originating from the editor.
/// </summary>
public class PasteEvent
{
    public Range? Range { get; set; }

    public string? LanguageId { get; set; }
}
