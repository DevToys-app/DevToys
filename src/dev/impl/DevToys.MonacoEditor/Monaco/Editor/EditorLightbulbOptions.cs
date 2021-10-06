#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Control the behavior and rendering of the code action lightbulb.
    ///
    /// Configuration options for editor lightbulb
    /// </summary>
    public sealed class EditorLightbulbOptions
    {
        /// <summary>
        /// Enable the lightbulb code action.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

}
