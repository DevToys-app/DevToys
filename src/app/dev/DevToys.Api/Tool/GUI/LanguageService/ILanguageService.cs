namespace DevToys.Api;

/// <summary>
/// Represents the declaration of a language service for <see cref="IUIMultiLineTextInput"/>.
/// </summary>
/// <remarks>
/// <para>
/// The value of <see cref="NameAttribute"/> should correspond to the <see cref="IUIMultiLineTextInput.SyntaxColorizationLanguageName"/>.
/// </para>
/// <example>
///     <code>
///         [Export(typeof(ILanguageService))]
///         [Name("programmingLanguageName")]
///         internal sealed class MyLanguageService : ILanguageService
///         {
///         }
///     </code>
/// </example>
/// </remarks>
public interface ILanguageService
{
    /// <summary>
    /// Gets the list of items to be displayed in the auto-completion list (invoked through Ctrl+Space in a <see cref="IUIMultiLineTextInput"/>).
    /// </summary>
    /// <param name="textDocument">The text in the <see cref="IUIMultiLineTextInput"/>.</param>
    /// <param name="span">The caret location, or selected span in the text document.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a list of items to display in the auto-completion popup.</returns>
    Task<IReadOnlyList<AutoCompletionItem>> GetAutoCompletionItemsAsync(string textDocument, TextSpan span, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an ordered list of semantic tokens for the specified text document.
    /// Semantic tokens are used to provide rich syntax highlighting and information for the <see cref="IUIMultiLineTextInput"/>'s editor.
    /// </summary>
    /// <param name="textDocument">The text in the <see cref="IUIMultiLineTextInput"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Returns a list of semantic tokens providing information about how the document should be colored</returns>
    /// <remarks>
    /// <para>
    /// Semantic tokens should be provided in the order in which they appear in the document. Their ranges should not overlap.
    /// Their positions should be relative to the previous token or the start of the document.
    /// </para>
    /// </remarks>
    Task<IReadOnlyList<SemanticToken>> GetSemanticTokensAsync(string textDocument, CancellationToken cancellationToken);
}
