#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// A structure defining a problem/warning/etc.
    /// </summary>
    public interface IMarkerData : IRange
    {
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        string? Code { get; set; }

        [JsonProperty("message")]
        string? Message { get; set; }

        [JsonProperty("relatedInformation", NullValueHandling = NullValueHandling.Ignore)]
        RelatedInformation[]? RelatedInformation { get; set; }

        [JsonProperty("severity")]
        MarkerSeverity Severity { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        string? Source { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        MarkerTag[]? Tags { get; set; }
    }
}
