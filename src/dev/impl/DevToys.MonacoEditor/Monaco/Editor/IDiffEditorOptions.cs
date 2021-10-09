#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configuration options for the diff editor.
    /// </summary>
    public interface IDiffEditorOptions : IEditorOptions
    {
        /// <summary>
        /// Allow the user to resize the diff editor split view.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("enableSplitViewResizing", NullValueHandling = NullValueHandling.Ignore)]
        bool? EnableSplitViewResizing { get; set; }
        /// <summary>
        /// Compute the diff by ignoring leading/trailing whitespace
        /// Defaults to true.
        /// </summary>
        [JsonProperty("ignoreTrimWhitespace", NullValueHandling = NullValueHandling.Ignore)]
        bool? IgnoreTrimWhitespace { get; set; }
        /// <summary>
        /// Timeout in milliseconds after which diff computation is cancelled.
        /// Defaults to 5000.
        /// </summary>
        [JsonProperty("maxComputationTime", NullValueHandling = NullValueHandling.Ignore)]
        uint? MaxComputationTime { get; set; }
        /// <summary>
        /// Original model should be editable?
        /// Defaults to false.
        /// </summary>
        [JsonProperty("originalEditable", NullValueHandling = NullValueHandling.Ignore)]
        bool? OriginalEditable { get; set; }
        /// <summary>
        /// Render +/- indicators for added/deleted changes.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("renderIndicators", NullValueHandling = NullValueHandling.Ignore)]
        bool? RenderIndicators { get; set; }
        /// <summary>
        /// Render the differences in two side-by-side editors.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("renderSideBySide", NullValueHandling = NullValueHandling.Ignore)]
        bool? RenderSideBySide { get; set; }
    }
}
