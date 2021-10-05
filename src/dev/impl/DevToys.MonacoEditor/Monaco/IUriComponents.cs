#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco
{
    public interface IUriComponents
    {
        [JsonProperty("authority", NullValueHandling = NullValueHandling.Ignore)]
        string? Authority { get; set; }

        [JsonProperty("fragment", NullValueHandling = NullValueHandling.Ignore)]
        string? Fragment { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        string? Path { get; set; }

        [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
        string? Query { get; set; }

        [JsonProperty("scheme", NullValueHandling = NullValueHandling.Ignore)]
        string? Scheme { get; set; }
    }
}
