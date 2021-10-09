#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configuration options for parameter hints
    /// </summary>
    public sealed class EditorParameterHintOptions
    {
        /// <summary>
        /// Enable cycling of parameter hints.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("cycle", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Cycle { get; set; }

        /// <summary>
        /// Enable parameter hints.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

}
