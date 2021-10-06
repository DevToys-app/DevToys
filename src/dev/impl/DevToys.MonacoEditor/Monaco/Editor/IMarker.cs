#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.imarker.html
    /// </summary>
    public interface IMarker : IMarkerData
    {
        [JsonProperty("owner")]
        string Owner { get; set; }

        [JsonProperty("resource")]
        Uri Resource { get; set; }
    }
}
