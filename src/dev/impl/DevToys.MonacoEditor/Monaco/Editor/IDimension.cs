#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// The initial editor dimension (to avoid measuring the container).
    /// </summary>
    public interface IDimension
    {
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        uint Height { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        uint Width { get; set; }
    }
}
