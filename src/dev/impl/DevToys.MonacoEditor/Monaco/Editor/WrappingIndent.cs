#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{

    /// <summary>
    /// Control indentation of wrapped lines. Can be: 'none', 'same', 'indent' or 'deepIndent'.
    /// Defaults to 'same' in vscode and to 'none' in monaco-editor.
    /// </summary>
    [JsonConverter(typeof(WrappingIndentConverter))]
    public enum WrappingIndent
    {
        DeepIndent,
        Indent,
        None,
        Same
    }
}
