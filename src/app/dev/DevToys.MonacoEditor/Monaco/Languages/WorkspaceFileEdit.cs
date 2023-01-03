using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public interface WorkspaceFileEdit
{
    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    WorkspaceEditMetadata Metadata { get; set; }

    [JsonProperty("newUri", NullValueHandling = NullValueHandling.Ignore)]
    Uri NewUri { get; set; }

    [JsonProperty("oldUri", NullValueHandling = NullValueHandling.Ignore)]
    Uri OldUri { get; set; }

    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    WorkspaceFileEditOptions Options { get; set; }
}
