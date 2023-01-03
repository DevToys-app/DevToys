using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public sealed class CodeAction
{
    [JsonProperty("command", NullValueHandling = NullValueHandling.Ignore)]
    public Command? Command { get; set; }

    [JsonProperty("diagnostics", NullValueHandling = NullValueHandling.Ignore)]
    public IMarkerData[]? Diagnostics { get; set; }

    [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
    public string? Disabled { get; set; }

    [JsonProperty("edit", NullValueHandling = NullValueHandling.Ignore)]
    public WorkspaceEdit? Edit { get; set; }

    [JsonProperty("isPreferred", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsPreferred { get; set; }

    [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
    public string? Kind { get; set; }

    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string? Title { get; set; }
}

