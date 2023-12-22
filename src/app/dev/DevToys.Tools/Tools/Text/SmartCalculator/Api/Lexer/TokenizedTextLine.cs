namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

/// <summary>
/// Represents a tokenized line in a text document.
/// </summary>
[DebuggerDisplay($"LineNumber = {{{nameof(LineNumber)}}}, LengthIncludingLineBreak = {{{nameof(LengthIncludingLineBreak)}}}")]
public sealed class TokenizedTextLine
{
    /// <summary>
    /// Gets the position at which the line starts.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// Gets the position at which the line ends, excluding the line break.
    /// </summary>
    public int End => Start + Length;

    /// <summary>
    /// Gets the position at which the line ends, including the line break.
    /// </summary>
    public int EndIncludingLineBreak => Start + LengthIncludingLineBreak;

    /// <summary>
    /// Gets the length of the line break.
    /// </summary>
    public int LineBreakLength { get; }

    /// <summary>
    /// Gets the length of the line, excluding the line break.
    /// </summary>
    public int Length => LengthIncludingLineBreak - LineBreakLength;

    /// <summary>
    /// Gets the length of the line, including the line break.
    /// </summary>
    public int LengthIncludingLineBreak { get; }

    /// <summary>
    /// Gets the line number in the text document.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets a <see cref="string"/> representation of the line, including the line break.
    /// </summary>
    public string LineTextIncludingLineBreak { get; }

    /// <summary>
    /// Gets the list of tokens in the line.
    /// </summary>
    public LinkedToken? Tokens { get; }

    internal TokenizedTextLine(
        int start,
        int lineNumber,
        int lineBreakLength,
        string? lineTextIncludingLineBreak,
        LinkedToken? tokens)
    {
        Guard.IsGreaterThanOrEqualTo(start, 0);
        Guard.IsGreaterThanOrEqualTo(lineNumber, 0);
        Guard.IsGreaterThanOrEqualTo(lineBreakLength, 0);

        Start = start;
        LineNumber = lineNumber;
        LineBreakLength = lineBreakLength;
        LineTextIncludingLineBreak = lineTextIncludingLineBreak ?? string.Empty;
        LengthIncludingLineBreak = LineTextIncludingLineBreak.Length;
        Tokens = tokens;
    }
}
