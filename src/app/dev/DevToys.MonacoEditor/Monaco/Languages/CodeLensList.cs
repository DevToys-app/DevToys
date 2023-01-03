namespace DevToys.MonacoEditor.Monaco.Languages;

using Newtonsoft.Json;

public sealed class CodeLensList // IDisposible?
{
    [JsonProperty("lenses", NullValueHandling = NullValueHandling.Ignore)]
    public CodeLens[]? Lenses { get; set; }
}

