namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

/// <summary>
/// Represents a token.
/// </summary>
public interface IToken
{
    /// <summary>
    /// Gets an internal non-localized, sensitive name that represents the type of token.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the position in the line where the token starts.
    /// </summary>
    public int StartInLine { get; }

    /// <summary>
    /// Gets the position in the line where the token ends.
    /// </summary>
    public int EndInLine { get; }

    /// <summary>
    /// Gets the length of the token.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets a <see cref="string"/> representation of the line, including the line break.
    /// </summary>
    public string LineTextIncludingLineBreak { get; }

    /// <summary>
    /// Determines whether the token's type is not equal to the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Case insensitive token type to compare with.</param>
    /// <returns>True if it is not equal</returns>
    bool IsNotOfType(string type);

    /// <summary>
    /// Determines whether the token's type is equal to the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Case insensitive token type to compare with.</param>
    /// <returns>True if it is equal</returns>
    bool IsOfType(string expectedType);

    /// <summary>
    /// Determines whether the token is equals to the given <paramref name="expectedType"/> and <paramref name="expectedTokenText"/>.
    /// </summary>
    bool Is(string expectedType, string expectedTokenText, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the token is equals to the given <paramref name="expectedType"/> and one of the <paramref name="expectedTokenTexts"/>.
    /// </summary>
    bool Is(string expectedType, string[] expectedTokenText, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the token's text is equals to the given text.
    /// </summary>
    bool IsTokenTextEqualTo(string compareTo, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the text of the token.
    /// </summary>
    /// <remarks>This method can be expensive as it generates a new <see cref="string"/>. Use it cautiously.</remarks>
    string GetText();

    /// <summary>
    /// Extract a text out of the <see cref="LineTextIncludingLineBreak"/>.
    /// </summary>
    /// <remarks>This method can be expensive as it generates a new <see cref="string"/>. Use it cautiously.</remarks>
    string GetText(int startInLine, int endInLine);
}
