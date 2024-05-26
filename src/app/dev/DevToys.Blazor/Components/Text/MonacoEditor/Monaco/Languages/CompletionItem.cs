///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Languages;

/// <summary>
/// A completion item represents a text snippet that is proposed to complete text that is being typed.
/// </summary>
public class CompletionItem
{
    /// <summary>
    /// The label of this completion item. By default
    /// this is also the text that is inserted when selecting
    /// this completion.
    /// </summary>
    public CompletionItemLabel? Label { get; set; }

    /// <summary>
    /// The kind of this completion item. Based on the kind an icon is chosen by the editor.
    /// </summary>
    public CompletionItemKind Kind { get; set; }

    /// <summary>
    /// A human-readable string that represents a doc-comment.
    /// </summary>
    public string? Documentation { get; set; }

    /// <summary>
    /// Select this item when showing. *Note* that only one completion item can be selected and
    /// that the editor decides which item that is. The rule is that the *first* item of those
    /// that match best is selected.
    /// </summary>
    public bool? Preselect { get; set; }

    /// <summary>
    /// A string or snippet that should be inserted in a document when selecting this completion.
    /// </summary>
    public string InsertText { get; set; } = string.Empty;

    /// <summary>
    /// Additional rules (as bitmask) that should be applied when inserting this completion.
    /// </summary>
    public CompletionItemInsertTextRule InsertTextRules { get; set; }

    /// <summary>
    /// A range of text that should be replaced by this completion item.
    /// 
    /// Defaults to a range from the start of the {@link TextDocument.getWordRangeAtPosition current word} to the
    /// current position.
    /// 
    /// Note:* The range must be a { @link Range.isSingleLine single line } and it must
    /// {@link Range.contains contain} the position at which completion has been {@link CompletionItemProvider.provideCompletionItems requested}.
    /// </summary>
    public Range? Range { get; set; }
}
