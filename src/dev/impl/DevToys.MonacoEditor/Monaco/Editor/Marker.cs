#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.imarker.html
    /// </summary>
    public sealed class Marker : IMarker
    {
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string? Code { get; set; }

        [JsonProperty("endColumn")]
        public uint EndColumn { get; set; }

        [JsonProperty("endLineNumber")]
        public uint EndLineNumber { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("owner")]
        public string? Owner { get; set; }

        [JsonProperty("relatedInformation", NullValueHandling = NullValueHandling.Ignore)]
        public RelatedInformation[]? RelatedInformation { get; set; }

        [JsonProperty("resource")]
        public Uri? Resource { get; set; }

        [JsonProperty("severity")]
        public MarkerSeverity Severity { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public string? Source { get; set; }

        [JsonProperty("startColumn")]
        public uint StartColumn { get; set; }

        [JsonProperty("startLineNumber")]
        public uint StartLineNumber { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public MarkerTag[]? Tags { get; set; }
    }
}
