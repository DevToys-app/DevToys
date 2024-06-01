///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Languages;

public class CompletionItemLabel
{
    public string Label { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public string? Description { get; set; }
}
