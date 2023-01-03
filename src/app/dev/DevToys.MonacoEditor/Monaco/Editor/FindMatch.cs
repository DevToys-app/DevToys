using DevToys.MonacoEditor.JsonConverters;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

public sealed class FindMatch
{
    [JsonProperty("matches", NullValueHandling = NullValueHandling.Ignore)]
    public string[]? Matches { get; set; }

    [JsonProperty("range", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(InterfaceToClassConverter<IRange, Range>))]
    public IRange? Range { get; set; }
}

