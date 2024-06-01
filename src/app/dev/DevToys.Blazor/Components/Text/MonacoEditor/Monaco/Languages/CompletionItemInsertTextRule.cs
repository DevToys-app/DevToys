///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Languages;

public enum CompletionItemInsertTextRule
{
    None = 0,

    /// <summary>
    /// Adjust whitespace/indentation of multiline insert texts to match the current line indentation.
    /// </summary>
    KeepWhitespace = 1,

    /// <summary>
    /// `insertText` is a snippet.
    /// </summary>
    InsertAsSnippet = 4
}
