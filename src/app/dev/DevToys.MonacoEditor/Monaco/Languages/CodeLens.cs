using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public sealed class CodeLens
{
    [JsonProperty("command", NullValueHandling = NullValueHandling.Ignore)]
    public Command? Command { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }

    [JsonProperty("range", NullValueHandling = NullValueHandling.Ignore)]
    public IRange? Range { get; set; }
}

