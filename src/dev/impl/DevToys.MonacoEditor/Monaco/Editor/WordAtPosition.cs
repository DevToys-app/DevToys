#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.iwordatposition.html
    /// </summary>
    public sealed class WordAtPosition : IWordAtPosition
    {
        /// <summary>
        /// Column where the word ends.
        /// </summary>
        [JsonProperty("endColumn")]
        public uint EndColumn { get; private set; }

        /// <summary>
        /// Column where the word starts.
        /// </summary>
        [JsonProperty("startColumn")]
        public uint StartColumn { get; private set; }

        /// <summary>
        /// The word.
        /// </summary>
        [JsonProperty("word")]
        public string? Word { get; private set; }
    }
}
