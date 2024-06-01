namespace DevToys.Api;

/// <summary>
/// Represents an item in the auto-completion list of <see cref="IUIMultiLineTextInput"/>.
/// </summary>
[DebuggerDisplay($"Title = {{{nameof(Title)}}}, Kind = {{{nameof(Kind)}}}")]
public class AutoCompletionItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCompletionItem"/> class.
    /// </summary>
    /// <param name="title">The title of the completion item.</param>
    /// <param name="kind">The kind of this completion item. Based on the kind an icon is chosen by the editor.</param>
    /// <param name="textToInsert">A snippet that should be inserted in a document when selecting this completion item.</param>
    public AutoCompletionItem(string title, AutoCompletionItemKind kind, string textToInsert)
    {
        Guard.IsNotNullOrEmpty(title);
        Guard.IsNotNull(textToInsert);
        Title = title;
        Kind = kind;
        TextToInsert = textToInsert;
    }

    /// <summary>
    /// Gets or sets the title of the completion item.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the completion item, displayed to the right of the suggestion list.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the kind of this completion item. Based on the kind an icon is chosen by the editor.
    /// </summary>
    public AutoCompletionItemKind Kind { get; set; }

    /// <summary>
    /// Gets or sets a human-readable string that represents a doc-comment. This string can be in Markdown format.
    /// </summary>
    public string? Documentation { get; set; }

    /// <summary>
    /// Gets or sets whether the item should be selected when showing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that only one completion item can be selected and that the editor decides which item that is.
    /// The rule is that the first item of those that match best is selected.
    /// </para>
    /// </remarks>
    public bool? ShouldBePreselected { get; set; }

    /// <summary>
    /// Gets or sets a snippet that should be inserted in a document when selecting this completion item.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use tab-stopped placeholders by using a syntax like <c>${1:foo}</c> and <c>${2:bar}</c>.
    /// </para>
    /// <para>
    /// Example:
    /// </para>
    /// <example>
    /// <code>
    /// registerCustomer(${1:firstName}, ${2:lastName})
    /// </code>
    /// </example>
    /// </remarks>
    public string TextToInsert { get; set; }
}
