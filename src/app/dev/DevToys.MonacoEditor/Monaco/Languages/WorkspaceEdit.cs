using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public sealed class WorkspaceEdit
{
    [JsonProperty("edits", NullValueHandling = NullValueHandling.Ignore)]
    public WorkspaceTextEdit[]? Edits { get; set; } // TODO: This could also be of type 'WorkspaceFileEdit'
}
