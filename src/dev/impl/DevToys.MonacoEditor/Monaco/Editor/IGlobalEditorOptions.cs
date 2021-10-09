#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Options which apply for all editors.
    /// </summary>
    public interface IGlobalEditorOptions
    {
        /// <summary>
        /// Controls whether `tabSize` and `insertSpaces` will be automatically detected when a file
        /// is opened based on the file contents.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("detectIndentation", NullValueHandling = NullValueHandling.Ignore)]
        bool? DetectIndentation { get; set; }

        /// <summary>
        /// Insert spaces when pressing `Tab`.
        /// This setting is overridden based on the file contents when `detectIndentation` is on.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("insertSpaces", NullValueHandling = NullValueHandling.Ignore)]
        bool? InsertSpaces { get; set; }

        /// <summary>
        /// Special handling for large files to disable certain memory intensive features.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("largeFileOptimizations", NullValueHandling = NullValueHandling.Ignore)]
        bool? LargeFileOptimizations { get; set; }

        /// <summary>
        /// Lines above this length will not be tokenized for performance reasons.
        /// Defaults to 20000.
        /// </summary>
        [JsonProperty("maxTokenizationLineLength", NullValueHandling = NullValueHandling.Ignore)]
        int? MaxTokenizationLineLength { get; set; }

        /// <summary>
        /// Keep peek editors open even when int clicking their content or when hitting `Escape`.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("stablePeek", NullValueHandling = NullValueHandling.Ignore)]
        bool? StablePeek { get; set; }

        /// <summary>
        /// The number of spaces a tab is equal to.
        /// This setting is overridden based on the file contents when `detectIndentation` is on.
        /// Defaults to 4.
        /// </summary>
        [JsonProperty("tabSize", NullValueHandling = NullValueHandling.Ignore)]
        int? TabSize { get; set; }

        /// <summary>
        /// Remove trailing auto inserted whitespace.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("trimAutoWhitespace", NullValueHandling = NullValueHandling.Ignore)]
        bool? TrimAutoWhitespace { get; set; }

        /// <summary>
        /// Controls whether completions should be computed based on words in the document.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("wordBasedSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        bool? WordBasedSuggestions { get; set; }
    }

}
