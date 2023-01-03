using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public interface WorkspaceEditMetadata
{
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    string Description { get; set; }

    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    string Label { get; set; }

    [JsonProperty("needsConfirmation", NullValueHandling = NullValueHandling.Ignore)]
    bool NeedsConfirmation { get; set; }
}