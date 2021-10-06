#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// A single edit operation, that acts as a simple replace.
    /// i.e. Replace text at `range` with `text` in model.
    /// </summary>
    public sealed class SingleEditOperation
    {
        /// <summary>
        /// This indicates that this operation has "insert" semantics.
        /// i.e. forceMoveMarkers = true => if `range` is collapsed, all markers at the position will
        /// be moved.
        /// </summary>
        [JsonProperty("forceMoveMarkers", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ForceMoveMarkers { get; set; }

        /// <summary>
        /// The range to replace. This can be empty to emulate a simple insert.
        /// </summary>
        [JsonProperty("range")]
        public IRange? Range { get; set; }

        /// <summary>
        /// The text to replace with. This can be null to emulate a simple delete.
        /// </summary>
        [JsonProperty("text")]
        public string? Text { get; set; }
    }
}