using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

/// <summary>
/// A service for tokenizing a text.
/// </summary>
public interface ILexer
{
    /// <summary>
    /// Tokenize the given text, line by line.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="wholeDocument">The text to tokenize.</param>
    IReadOnlyList<TokenizedTextLine> Tokenize(string culture, string? wholeDocument);
}
