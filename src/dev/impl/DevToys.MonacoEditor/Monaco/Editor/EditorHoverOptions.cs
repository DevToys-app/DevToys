#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configure the editor's hover.
    ///
    /// Configuration options for editor hover
    /// </summary>
    public sealed class EditorHoverOptions
    {
        /// <summary>
        /// Delay for showing the hover.
        /// Defaults to 300.
        /// </summary>
        [JsonProperty("delay", NullValueHandling = NullValueHandling.Ignore)]
        public int? Delay { get; set; }

        /// <summary>
        /// Enable the hover.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Is the hover sticky such that it can be clicked and its contents selected?
        /// Defaults to true.
        /// </summary>
        [JsonProperty("sticky", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Sticky { get; set; }
    }

}
