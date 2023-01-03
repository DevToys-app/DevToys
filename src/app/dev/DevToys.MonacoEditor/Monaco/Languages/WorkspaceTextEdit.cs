using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public sealed class WorkspaceTextEdit
{
    [JsonProperty("edit", NullValueHandling = NullValueHandling.Ignore)]
    public TextEdit? Edit { get; set; }

    [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
    public WorkspaceEditMetadata? Metadata { get; set; }

    [JsonProperty("modelVersionId", NullValueHandling = NullValueHandling.Ignore)]
    public double? ModelVersionId { get; set; } //// Important for this to be nullable here, as otherwise it still serializes as '0' which throws a 'bad state - model changed in the meantime' as version will mismatch

    [JsonProperty("resource", NullValueHandling = NullValueHandling.Ignore)]
    public Uri? Resource { get; set; }
}
