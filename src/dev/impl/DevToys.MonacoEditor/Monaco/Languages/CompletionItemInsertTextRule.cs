#nullable enable

namespace DevToys.MonacoEditor.Monaco.Languages
{
    public enum CompletionItemInsertTextRule
    {
        /// <summary>
        /// `insertText` is a snippet.
        /// </summary>
        InsertAsSnippet = 4,

        /// <summary>
        /// Adjust whitespace/indentation of multiline insert texts to
        /// match the current line indentation.
        /// </summary>
        KeepWhitespace = 1
    }
}
