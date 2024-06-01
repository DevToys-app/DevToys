namespace DevToys.Api;

/// <summary>
/// Represents a semantic token.
/// </summary>
[DebuggerDisplay($"Line = {{{nameof(DeltaLine)}}}, Column = {{{nameof(DeltaColumn)}}}, Length = {{{nameof(Length)}}}, Type = {{{nameof(TokenType)}}}")]
public class SemanticToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticToken"/> class.
    /// </summary>
    /// <param name="deltaLine">The token line number, relative to the previous token.</param>
    /// <param name="deltaColumn">The token start character in the line, relative to the previous token</param>
    /// <param name="length">The length of the token.</param>
    /// <param name="tokenType">The token type.</param>
    /// <param name="tokenModifier">The token modifier.</param>
    public SemanticToken(int deltaLine, int deltaColumn, int length, SemanticTokenType tokenType, SemanticTokenModifier tokenModifier = SemanticTokenModifier.Declaration)
    {
        Guard.IsGreaterThanOrEqualTo(deltaLine, 0);
        Guard.IsGreaterThanOrEqualTo(deltaColumn, 0);
        Guard.IsGreaterThanOrEqualTo(length, 0);
        DeltaLine = deltaLine;
        DeltaColumn = deltaColumn;
        Length = length;
        TokenType = tokenType;
        TokenModifier = tokenModifier;
    }

    /// <summary>
    /// Gets or sets the token line number, relative to the previous token.
    /// </summary>
    public int DeltaLine { get; set; }

    /// <summary>
    /// Gets or sets the token start character in the line, relative to the previous token (relative to 0 or the previous token's start if they are on the same line).
    /// </summary>
    public int DeltaColumn { get; set; }

    /// <summary>
    /// Gets or sets the length of the token. A token cannot be multiline.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    public SemanticTokenType TokenType { get; set; }

    /// <summary>
    /// Gets or sets the token modifier.
    /// </summary>
    public SemanticTokenModifier TokenModifier { get; set; }
}
