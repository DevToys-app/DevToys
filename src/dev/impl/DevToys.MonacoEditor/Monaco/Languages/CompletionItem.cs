#nullable enable

using DevToys.MonacoEditor.Monaco.Editor;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages
{
    /// <summary>
    /// A completion item represents a text snippet that is
    /// proposed to complete text that is being typed.
    /// </summary>
    public sealed class CompletionItem
    {
        /// <summary>
        /// An optional array of additional text edits that are applied when
        /// selecting this completion. Edits must not overlap with the main edit
        /// nor with themselves.
        /// </summary>
        [JsonProperty("additionalTextEdits", NullValueHandling = NullValueHandling.Ignore)]
        public SingleEditOperation[]? AdditionalTextEdits { get; set; }

        /// <summary>
        /// A command that should be run upon acceptance of this item.
        /// </summary>
        [JsonProperty("command", NullValueHandling = NullValueHandling.Ignore)]
        public Command? Command { get; set; }

        /// <summary>
        /// An optional set of characters that when pressed while this completion is active will
        /// accept it first and
        /// then type that character. *Note* that all commit characters should have `length=1` and
        /// that superfluous
        /// characters will be ignored.
        /// </summary>
        [JsonProperty("commitCharacters", NullValueHandling = NullValueHandling.Ignore)]
        public string[]? CommitCharacters { get; set; }

        /// <summary>
        /// A human-readable string with additional information
        /// about this item, like type or symbol information.
        /// </summary>
        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string? Detail { get; set; }

        /// <summary>
        /// A human-readable string that represents a doc-comment.
        /// </summary>
        [JsonProperty("documentation", NullValueHandling = NullValueHandling.Ignore)]
        public MarkdownString? Documentation { get; set; }

        /// <summary>
        /// A string that should be used when filtering a set of
        /// completion items. When `falsy` the [label](#CompletionItem.label)
        /// is used.
        /// </summary>
        [JsonProperty("filterText", NullValueHandling = NullValueHandling.Ignore)]
        public string? FilterText { get; set; }

        /// <summary>
        /// A string or snippet that should be inserted in a document when selecting
        /// this completion. When `falsy` the [label](#CompletionItem.label)
        /// is used.
        /// </summary>
        [JsonProperty("insertText")]
        public string? InsertText { get; set; }

        /// <summary>
        /// Addition rules (as bitmask) that should be applied when inserting
        /// this completion.
        /// </summary>
        [JsonProperty("insertTextRules", NullValueHandling = NullValueHandling.Ignore)]
        public CompletionItemInsertTextRule? InsertTextRules { get; set; }

        /// <summary>
        /// The kind of this completion item. Based on the kind
        /// an icon is chosen by the editor.
        /// </summary>
        [JsonProperty("kind")]
        public CompletionItemKind Kind { get; set; }

        /// <summary>
        /// The label of this completion item. By default
        /// this is also the text that is inserted when selecting
        /// this completion.
        /// </summary>
        [JsonProperty("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Select this item when showing. *Note* that only one completion item can be selected and
        /// that the editor decides which item that is. The rule is that the *first* item of those
        /// that match best is selected.
        /// </summary>
        [JsonProperty("preselect", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Preselect { get; set; }

        /// <summary>
        /// A range of text that should be replaced by this completion item.
        ///
        /// Defaults to a range from the start of the [current
        /// word](#TextDocument.getWordRangeAtPosition) to the
        /// current position.
        ///
        /// *Note:* The range must be a [single line](#Range.isSingleLine) and it must
        /// [contain](#Range.contains) the position at which completion has been
        /// [requested](#CompletionItemProvider.provideCompletionItems).
        /// </summary>
        [JsonProperty("range")]
        public Range? Range { get; set; }

        /// <summary>
        /// A string that should be used when comparing this item
        /// with other items. When `falsy` the [label](#CompletionItem.label)
        /// is used.
        /// </summary>
        [JsonProperty("sortText", NullValueHandling = NullValueHandling.Ignore)]
        public string? SortText { get; set; }

        /// <summary>
        /// A modifier to the `kind` which affect how the item
        /// is rendered, e.g. Deprecated is rendered with a strikeout
        /// </summary>
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public MarkerTag[]? Tags { get; set; }

        public CompletionItem(string label, string insertText, CompletionItemKind kind)
        {
            InsertText = insertText;
            Label = label;
            Kind = kind;
        }
    }
}
