using System.Text;
using DevToys.Tools.Models;

namespace DevToys.Tools.Helpers;

internal static class MemoryExtensions
{
    /// <summary>
    /// Detects whether the given location in a span of characters is a line break.
    /// </summary>
    internal static bool IsLineBreak(this Span<char> span, int i, out EndOfLineSequence lineBreakType)
    {
        return ((ReadOnlySpan<char>)span).IsLineBreak(i, out lineBreakType);
    }

    /// <summary>
    /// Detects whether the given location in a span of characters is a line break.
    /// </summary>
    internal static bool IsLineBreak(this ReadOnlySpan<char> span, int i, out EndOfLineSequence lineBreakType)
    {
        if (i < span.Length)
        {
            char c1 = span[i];
            if (c1 == '\n')
            {
                lineBreakType = EndOfLineSequence.LineFeed;
                return true;
            }

            if (c1 == '\r')
            {
                if (i < span.Length - 1 && span[i + 1] == '\n')
                {
                    lineBreakType = EndOfLineSequence.CarriageReturnLineFeed;
                }
                else
                {
                    lineBreakType = EndOfLineSequence.CarriageReturn;
                }

                return true;
            }
        }

        lineBreakType = EndOfLineSequence.Unknown;
        return false;
    }

    /// <summary>
    /// Gets the last word in a span of characters. This takes into account the fact that the last word may be before a word delimiter.
    /// For example, in the sentence "Hello world.", the last word is "world", but the last character is a dot.
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    internal static ReadOnlySpan<char> GetLastWord(this ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
        {
            return span;
        }

        // Find the last word. We start reading the span from the end, and stop when we find a word delimiter such as a dot or whitespace.
        // If the last character of the span is a word delimiter, we slice the span from the index before the last delimiter.
        int spanEndOfLastWord = -1;
        int spanBeginningOfLastWord = -1;
        for (int i = span.Length - 1; i >= 0; i--)
        {
            if (spanEndOfLastWord == -1)
            {
                if (char.IsLetterOrDigit(span[i]))
                {
                    spanEndOfLastWord = i + 1;
                }
            }
            else if (!char.IsLetterOrDigit(span[i]))
            {
                spanBeginningOfLastWord = i + 1;
                break;
            }
            else
            {
                spanBeginningOfLastWord = i;
            }
        }

        if (spanBeginningOfLastWord == -1 || spanEndOfLastWord == -1)
        {
            spanBeginningOfLastWord = 0;
            spanEndOfLastWord = 0;
        }

        return span.Slice(spanBeginningOfLastWord, spanEndOfLastWord - spanBeginningOfLastWord);
    }

    /// <summary>
    /// Splits the given span of characters into lines.
    /// </summary>
    internal static List<ReadOnlyMemory<char>> ToLines(this ReadOnlyMemory<char> span)
    {
        // Split text by lines.
        var lines = new List<ReadOnlyMemory<char>>();

        SpanLineEnumerator lineEnumerator = span.Span.EnumerateLines(); // This supports every kind of line break.

        while (lineEnumerator.MoveNext())
        {
            ReadOnlySpan<char> line = lineEnumerator.Current;
            char[] lineBuffer = new char[line.Length];
            line.CopyTo(lineBuffer);
            lines.Add(lineBuffer);
        }

        return lines;
    }

    /// <summary>
    /// Rebuilds a string from a list of lines.
    /// </summary>
    internal static string BuildStringFromListOfLines(this IReadOnlyList<ReadOnlyMemory<char>> lines, EndOfLineSequence endOfLineSequence)
    {
        var stringBuilder = new StringBuilder();
        string endOfLineSequenceString = endOfLineSequence.ToLineBreakCharacters();

        for (int i = 0; i < lines.Count; i++)
        {
            stringBuilder.Append(lines[i].Span);
            if (i < lines.Count - 1)
            {
                stringBuilder.Append(endOfLineSequenceString);
            }
        }

        return stringBuilder.ToString();
    }
}
