using System.Text;
using System.Text.RegularExpressions;
using DevToys.Tools.Helpers.Core;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class StringHelper
{
    [GeneratedRegex(@"\\[nrtfb\\""]")]
    private static partial Regex EscapeCharactersRegex();

    internal static bool HasEscapeCharacters(string text)
    {
        return EscapeCharactersRegex().IsMatch(text);
    }

    internal static ResultInfo<string> EscapeString(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return new(string.Empty, HasSucceeded: true);
        }

        var encoded = new StringBuilder();

        try
        {
            int i = 0;
            while (i < data!.Length)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new(string.Empty, HasSucceeded: false);
                }

                string replacementString = string.Empty;
                int jumpLength = 0;
                if (TextMatchAtIndex(data, "\n", i))
                {
                    jumpLength = 1;
                    replacementString = "\\n";
                }
                else if (TextMatchAtIndex(data, "\r", i))
                {
                    jumpLength = 1;
                    replacementString = "\\r";
                }
                else if (TextMatchAtIndex(data, "\t", i))
                {
                    jumpLength = 1;
                    replacementString = "\\t";
                }
                else if (TextMatchAtIndex(data, "\b", i))
                {
                    jumpLength = 1;
                    replacementString = "\\b";
                }
                else if (TextMatchAtIndex(data, "\f", i))
                {
                    jumpLength = 1;
                    replacementString = "\\f";
                }
                else if (TextMatchAtIndex(data, "\"", i))
                {
                    jumpLength = 1;
                    replacementString = "\\\"";
                }
                else if (TextMatchAtIndex(data, "\\", i))
                {
                    jumpLength = 1;
                    replacementString = "\\\\";
                }

                if (!string.IsNullOrEmpty(replacementString) && jumpLength > 0)
                {
                    encoded.Append(replacementString);
                    i += jumpLength;
                }
                else
                {
                    encoded.Append(data[i]);
                    i++;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to escape text");
            return new(ex.Message, HasSucceeded: false);
        }

        return new(encoded.ToString(), HasSucceeded: true);
    }

    internal static ResultInfo<string> UnescapeString(string? data, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return new(string.Empty, HasSucceeded: false);
        }

        var decoded = new StringBuilder();

        try
        {
            int i = 0;
            while (i < data!.Length)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new(string.Empty, HasSucceeded: false);
                }

                string replacementString = string.Empty;
                int jumpLength = 0;
                if (TextMatchAtIndex(data, "\\n", i))
                {
                    jumpLength = 2;
                    replacementString = "\n";
                }
                else if (TextMatchAtIndex(data, "\\r", i))
                {
                    jumpLength = 2;
                    replacementString = "\r";
                }
                else if (TextMatchAtIndex(data, "\\t", i))
                {
                    jumpLength = 2;
                    replacementString = "\t";
                }
                else if (TextMatchAtIndex(data, "\\b", i))
                {
                    jumpLength = 2;
                    replacementString = "\b";
                }
                else if (TextMatchAtIndex(data, "\\f", i))
                {
                    jumpLength = 2;
                    replacementString = "\f";
                }
                else if (TextMatchAtIndex(data, "\\\"", i))
                {
                    jumpLength = 2;
                    replacementString = "\"";
                }
                else if (TextMatchAtIndex(data, "\\\\", i))
                {
                    jumpLength = 2;
                    replacementString = "\\";
                }

                if (!string.IsNullOrEmpty(replacementString) && jumpLength > 0)
                {
                    decoded.Append(replacementString);
                    i += jumpLength;
                }
                else
                {
                    decoded.Append(data[i]);
                    i++;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to escape text");
            return new(ex.Message, HasSucceeded: false);
        }

        return new(decoded.ToString(), HasSucceeded: true);
    }

    internal static string SortLinesAlphabetically(string text, EndOfLineSequence endOfLineSequence)
    {
        return SortLines(
            text,
            lineComparer: (ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) // Order alphabetically.
                => System.MemoryExtensions.CompareTo(x.Span, y.Span, StringComparison.CurrentCulture),
            endOfLineSequence);
    }

    internal static string SortLinesAlphabeticallyDescending(string text, EndOfLineSequence endOfLineSequence)
    {
        return SortLines(
            text,
            lineComparer: (ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) // Order reversed alphabetically.
                => System.MemoryExtensions.CompareTo(y.Span, x.Span, StringComparison.CurrentCulture),
            endOfLineSequence);
    }

    internal static string SortLinesByLastWord(string text, EndOfLineSequence endOfLineSequence)
    {
        return SortLines(
            text,
            lineComparer: (ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => // Order alphabetically from the end of the line.
            {
                ReadOnlySpan<char> span1LastWord = x.Span.GetLastWord();
                ReadOnlySpan<char> span2LastWord = y.Span.GetLastWord();

                return System.MemoryExtensions.CompareTo(span1LastWord, span2LastWord, StringComparison.CurrentCulture);
            },
            endOfLineSequence);
    }

    internal static string SortLinesByLastWordDescending(string text, EndOfLineSequence endOfLineSequence)
    {
        return SortLines(
            text,
            lineComparer: (ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => // Order alphabetically from the end of the line.
            {
                ReadOnlySpan<char> span1LastWord = x.Span.GetLastWord();
                ReadOnlySpan<char> span2LastWord = y.Span.GetLastWord();

                return System.MemoryExtensions.CompareTo(span2LastWord, span1LastWord, StringComparison.CurrentCulture);
            },
            endOfLineSequence);
    }

    internal static string ReverseLines(string text, EndOfLineSequence endOfLineSequence)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // Split text by lines.
        List<ReadOnlyMemory<char>> lines = text.AsMemory().ToLines();

        // Reverse the order of the list.
        lines.Reverse();

        // Rebuilt the text.
        return lines.BuildStringFromListOfLines(endOfLineSequence);
    }

    internal static string ShuffleLines(string text, EndOfLineSequence endOfLineSequence)
    {
        return SortLines(
            text,
            lineComparer: (ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) => // Order randomly.
            {
                return Random.Shared.Next(-1, 1);
            },
            endOfLineSequence);
    }

    internal static string ConvertLineBreakToLF(string text)
    {
        Guard.IsNotNull(text);
        return Regex.Replace(text, @"\r\n?|\n", "\n");
    }

    internal static string ConvertLineBreakToCRLF(string text)
    {
        Guard.IsNotNull(text);
        return Regex.Replace(text, @"\r\n?|\n", "\r\n");
    }

    internal static string ConvertToSentenceCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);

        if (text.Length == 0)
        {
            return text;
        }

        return string.Create(text.Length, (text, cancellationToken), static (buffer, state) =>
        {
            ReadOnlySpan<char> textSpan = state.text.AsSpan();

            bool newSentence = true;
            for (int i = 0; i < textSpan.Length; i++)
            {
                state.cancellationToken.ThrowIfCancellationRequested();
                char @char = textSpan[i];

                EndOfLineSequence lineBreakType = EndOfLineSequence.Unknown;
                if (IsSentenceTerminator(@char) || textSpan.IsLineBreak(i, out lineBreakType))
                {
                    buffer[i] = @char;
                    newSentence = true;
                    if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                    {
                        i++;
                        buffer[i] = textSpan[i];
                    }
                    continue;
                }

                if (char.IsLetterOrDigit(@char))
                {
                    if (newSentence)
                    {
                        buffer[i] = char.ToUpperInvariant(@char);
                        newSentence = false;
                    }
                    else
                    {
                        buffer[i] = char.ToLowerInvariant(@char);
                    }
                    continue;
                }

                buffer[i] = @char;
            }
        });
    }

    internal static string ConvertToTitleCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);

        if (text.Length == 0)
        {
            return text;
        }

        return string.Create(text.Length, (text, cancellationToken), static (buffer, state) =>
        {
            ReadOnlySpan<char> textSpan = state.text.AsSpan();

            for (int i = 0; i < textSpan.Length; i++)
            {
                state.cancellationToken.ThrowIfCancellationRequested();

                if (i == 0 || !char.IsLetterOrDigit(textSpan[i - 1]))
                {
                    buffer[i] = char.ToUpperInvariant(textSpan[i]);
                }
                else
                {
                    buffer[i] = char.ToLowerInvariant(textSpan[i]);
                }
            }
        });
    }

    internal static string ConvertToCamelCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);
        if (text.Length == 0)
        {
            return text;
        }

        var camelCaseStringBuilder = new StringBuilder();
        bool nextLetterOrDigitShouldBeUppercase = false;

        ReadOnlySpan<char> span = text.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            char currentChar = span[i];
            if (char.IsLetterOrDigit(currentChar))
            {
                if (nextLetterOrDigitShouldBeUppercase)
                {
                    camelCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                    nextLetterOrDigitShouldBeUppercase = false;
                }
                else
                {
                    camelCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                }
            }
            else
            {
                if (span.IsLineBreak(i, out EndOfLineSequence lineBreakType))
                {
                    nextLetterOrDigitShouldBeUppercase = false;
                    camelCaseStringBuilder.Append(currentChar);
                    if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                    {
                        i++;
                        currentChar = span[i];
                        camelCaseStringBuilder.Append(currentChar);
                    }
                }
                else
                {
                    nextLetterOrDigitShouldBeUppercase = true;
                }
            }
        }

        return camelCaseStringBuilder.ToString();
    }

    internal static string ConvertToPascalCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);
        if (text.Length == 0)
        {
            return text;
        }

        var pascalCaseStringBuilder = new StringBuilder();
        bool nextLetterOrDigitShouldBeUppercase = true;

        ReadOnlySpan<char> span = text.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            char currentChar = span[i];
            if (char.IsLetterOrDigit(currentChar))
            {
                if (nextLetterOrDigitShouldBeUppercase)
                {
                    pascalCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                    nextLetterOrDigitShouldBeUppercase = false;
                }
                else
                {
                    pascalCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                }
            }
            else
            {
                nextLetterOrDigitShouldBeUppercase = true;
                if (span.IsLineBreak(i, out EndOfLineSequence lineBreakType))
                {
                    pascalCaseStringBuilder.Append(currentChar);
                    if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                    {
                        i++;
                        currentChar = span[i];
                        pascalCaseStringBuilder.Append(currentChar);
                    }
                }
            }
        }

        return pascalCaseStringBuilder.ToString();
    }

    internal static string ConvertToSnakeCase(string text, CancellationToken cancellationToken)
    {
        return SnakeConstantKebabCobolCaseConverter(text, '_', isUpperCase: false, cancellationToken);
    }

    internal static string ConvertToConstantCase(string text, CancellationToken cancellationToken)
    {
        return SnakeConstantKebabCobolCaseConverter(text, '_', isUpperCase: true, cancellationToken);
    }

    internal static string ConvertToKebabCase(string text, CancellationToken cancellationToken)
    {
        return SnakeConstantKebabCobolCaseConverter(text, '-', isUpperCase: false, cancellationToken);
    }

    internal static string ConvertToCobolCase(string text, CancellationToken cancellationToken)
    {
        return SnakeConstantKebabCobolCaseConverter(text, '-', isUpperCase: true, cancellationToken);
    }

    internal static string ConvertToTrainCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);
        if (text.Length == 0)
        {
            return text;
        }

        var snakeCaseStringBuilder = new StringBuilder();
        ReadOnlySpan<char> span = text.AsSpan();

        bool nextNonLetterOrDigitShouldBeIgnored = true;
        for (int i = 0; i < span.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            char currentChar = span[i];
            if (char.IsLetterOrDigit(currentChar))
            {
                if (nextNonLetterOrDigitShouldBeIgnored)
                {
                    snakeCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                }
                else
                {
                    snakeCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                }
                nextNonLetterOrDigitShouldBeIgnored = false;
            }
            else if (span.IsLineBreak(i, out EndOfLineSequence lineBreakType))
            {
                nextNonLetterOrDigitShouldBeIgnored = true;
                snakeCaseStringBuilder.Append(currentChar);
                if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                {
                    i++;
                    currentChar = span[i];
                    snakeCaseStringBuilder.Append(currentChar);
                }
            }
            else if (!nextNonLetterOrDigitShouldBeIgnored)
            {
                if (i < span.Length - 1
                    && char.IsLetterOrDigit(span[i + 1]))
                {
                    nextNonLetterOrDigitShouldBeIgnored = true;
                    snakeCaseStringBuilder.Append('-');
                }
            }
        }

        return snakeCaseStringBuilder.ToString();
    }

    internal static string ConvertToAlternatingCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);

        if (text.Length == 0)
        {
            return text;
        }

        return string.Create(text.Length, (text, cancellationToken), static (buffer, state) =>
        {
            ReadOnlySpan<char> textSpan = state.text.AsSpan();

            bool lowerCase = true;
            for (int i = 0; i < textSpan.Length; i++)
            {
                state.cancellationToken.ThrowIfCancellationRequested();

                buffer[i] = lowerCase ? char.ToLowerInvariant(textSpan[i]) : char.ToUpperInvariant(textSpan[i]);

                lowerCase = !lowerCase;
            }
        });
    }

    internal static string ConvertToInverseCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);

        if (text.Length == 0)
        {
            return text;
        }

        return string.Create(text.Length, (text, cancellationToken), static (buffer, state) =>
        {
            ReadOnlySpan<char> textSpan = state.text.AsSpan();

            bool lowerCase = false;
            for (int i = 0; i < textSpan.Length; i++)
            {
                state.cancellationToken.ThrowIfCancellationRequested();

                buffer[i] = lowerCase ? char.ToLowerInvariant(textSpan[i]) : char.ToUpperInvariant(textSpan[i]);

                lowerCase = !lowerCase;
            }
        });
    }

    internal static string ConvertToRandomCase(string text, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);

        if (text.Length == 0)
        {
            return text;
        }

        return string.Create(text.Length, (text, cancellationToken), static (buffer, state) =>
        {
            ReadOnlySpan<char> textSpan = state.text.AsSpan();

            for (int i = 0; i < textSpan.Length; i++)
            {
                state.cancellationToken.ThrowIfCancellationRequested();

                bool isUpper = Random.Shared.Next() % 2 == 0;

                buffer[i] = isUpper ? char.ToUpperInvariant(textSpan[i]) : char.ToLowerInvariant(textSpan[i]);
            }
        });
    }

    internal static EndOfLineSequence DetectLineBreakKind(string text, CancellationToken cancellationToken)
    {
        EndOfLineSequence overallLineBreakType = EndOfLineSequence.Unknown;
        ReadOnlySpan<char> span = text.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (span.IsLineBreak(i, out EndOfLineSequence lineBreakType))
            {
                // Detect the overall line break type of the text we're analyzing.
                if (overallLineBreakType == EndOfLineSequence.Unknown)
                {
                    overallLineBreakType = lineBreakType;
                }
                else if (overallLineBreakType != lineBreakType)
                {
                    overallLineBreakType = EndOfLineSequence.Mixed;
                    break;
                }

                if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                {
                    // If we found \r\n, skip the '\n' character.
                    i++;
                    continue;
                }
            }
        }

        return overallLineBreakType;
    }

    internal static void AnalyzeSpan(
        string text,
        TextSpan span,
        CancellationToken cancellationToken,
        out int spanLength,
        out int spanStartPosition,
        out int spanEndPosition,
        out int spanLineNumber,
        out int spanColumnNumber)
    {
        Guard.IsNotNull(text);
        spanLength = span.Length;
        spanStartPosition = span.StartPosition;
        spanEndPosition = span.EndPosition;
        spanLineNumber = CountLines(text, span.StartPosition);
        int beginningOfLinePosition = text.LastIndexOf('\n', Math.Max(0, Math.Min(span.StartPosition - 1, text.Length - 1)));
        if (beginningOfLinePosition == -1)
        {
            spanColumnNumber = span.StartPosition;
        }
        else
        {
            spanColumnNumber = Math.Max(span.StartPosition - beginningOfLinePosition - 1, 0);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    internal static void AnalyzeText(
        string text,
        CancellationToken cancellationToken,
        out int byteCount,
        out int characterCount,
        out int wordCount,
        out int sentenceCount,
        out int paragraphCount,
        out int lineCount,
        out EndOfLineSequence overallLineBreakType,
        out Dictionary<char, int> characterFrequency,
        out Dictionary<string, int> wordFrequency)
    {
        Guard.IsNotNull(text);
        byteCount = Encoding.UTF8.GetByteCount(text);
        characterCount = text.Length;
        wordCount = 0;
        sentenceCount = 0;
        lineCount = 1;
        paragraphCount = 1;
        overallLineBreakType = EndOfLineSequence.Unknown;
        characterFrequency = new Dictionary<char, int>();
        wordFrequency = new Dictionary<string, int>();

        int sentenceBeginningPosition = 0;
        int consecutiveLineBreakCount = 0;
        bool isWordStart = true;
        var wordBuilder = new StringBuilder();

        ReadOnlySpan<char> span = text.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            char c = span[i];

            // Update the character distribution
            UpdateCharacterFrequency(characterFrequency, c);

            bool isWordSeparator = IsWordSeparator(c);

            // If the current character is not a word separator, append it to the word builder
            if (!isWordSeparator)
            {
                wordBuilder.Append(c);

                // If the current character is the start of a word, increment the word count
                if (isWordStart)
                {
                    wordCount++;
                }
            }
            else if (wordBuilder.Length > 0)
            {
                // Otherwise, we reached the end of a word, let's update the word distribution
                UpdateWordFrequency(wordFrequency, wordBuilder.ToString());
                wordBuilder.Clear();
            }

            isWordStart = isWordSeparator;

            // If we found a line break, increment the line count
            if (span.IsLineBreak(i, out EndOfLineSequence lineBreakType))
            {
                lineCount++;
                consecutiveLineBreakCount++;

                // Detect the overall line break type of the text we're analyzing.
                if (overallLineBreakType == EndOfLineSequence.Unknown)
                {
                    overallLineBreakType = lineBreakType;
                }
                else if (overallLineBreakType != lineBreakType)
                {
                    overallLineBreakType = EndOfLineSequence.Mixed;
                }

                if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                {
                    // If we found \r\n, skip the '\n' character.
                    i++;
                    continue;
                }
            }
            else
            {
                // If there are more than one consecutive line breaks, we have a paragraph.
                if (consecutiveLineBreakCount > 1)
                {
                    paragraphCount++;
                }

                consecutiveLineBreakCount = 0;
            }

            // Detect sentences.
            bool isSentenceTerminator = IsSentenceTerminator(c);
            if (isSentenceTerminator && !IsSpanEmptyOrNotLetterAndDigit(text, TextSpan.FromBounds(sentenceBeginningPosition, i)))
            {
                sentenceCount++;
                sentenceBeginningPosition = i + 1;
            }
        }

        // Detect final sentences.
        if (sentenceBeginningPosition < text.Length - 1
            && !IsSpanEmptyOrNotLetterAndDigit(text, TextSpan.FromBounds(sentenceBeginningPosition, text.Length - 1)))
        {
            sentenceCount++;
        }
    }

    private static void UpdateCharacterFrequency(Dictionary<char, int> characterDistribution, char c)
    {
        if (characterDistribution.TryGetValue(c, out int count))
        {
            characterDistribution[c] = ++count;
        }
        else
        {
            characterDistribution[c] = 1;
        }
    }

    private static void UpdateWordFrequency(Dictionary<string, int> wordDistribution, string word)
    {
        if (wordDistribution.TryGetValue(word, out int value))
        {
            wordDistribution[word] = ++value;
        }
        else
        {
            wordDistribution[word] = 1;
        }
    }

    private static bool IsWordSeparator(char c)
    {
        return char.IsWhiteSpace(c) || char.IsPunctuation(c);
    }

    private static bool IsSentenceTerminator(char c)
    {
        return c == '.' || c == '?' || c == '!';
    }

    private static bool IsSpanEmptyOrNotLetterAndDigit(string text, TextSpan span)
    {
        Guard.IsInRange(span.StartPosition, 0, text.Length, nameof(span));
        Guard.IsInRange(span.EndPosition, 0, text.Length, nameof(span));

        for (int i = span.StartPosition; i < span.EndPosition; i++)
        {
            char currentChar = text[i];
            if (char.IsLetterOrDigit(currentChar))
            {
                return false;
            }
        }

        return true;
    }

    private static int CountLines(string text, int length)
    {
        int lineCount = 1;

        if (string.IsNullOrEmpty(text))
        {
            return lineCount;
        }

        int index = -1;
        while (-1 != (index = text.IndexOf('\n', index + 1, length - (index + 1))))
        {
            lineCount++;
        }

        return lineCount;
    }

    private static string SnakeConstantKebabCobolCaseConverter(string text, char spaceReplacement, bool isUpperCase, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(text);
        if (text.Length == 0)
        {
            return text;
        }

        var snakeCaseStringBuilder = new StringBuilder();
        ReadOnlySpan<char> span = text.AsSpan();

        bool nextNonLetterOrDigitShouldBeIgnored = true;
        for (int i = 0; i < span.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            char currentChar = span[i];
            if (char.IsLetterOrDigit(currentChar))
            {
                nextNonLetterOrDigitShouldBeIgnored = false;
                if (isUpperCase)
                {
                    snakeCaseStringBuilder.Append(char.ToUpperInvariant(currentChar));
                }
                else
                {
                    snakeCaseStringBuilder.Append(char.ToLowerInvariant(currentChar));
                }
            }
            else if (span.IsLineBreak(i, out EndOfLineSequence lineBreakType))
            {
                nextNonLetterOrDigitShouldBeIgnored = true;
                snakeCaseStringBuilder.Append(currentChar);
                if (lineBreakType == EndOfLineSequence.CarriageReturnLineFeed)
                {
                    i++;
                    currentChar = span[i];
                    snakeCaseStringBuilder.Append(currentChar);
                }
            }
            else if (!nextNonLetterOrDigitShouldBeIgnored)
            {
                if (i < span.Length - 1
                    && char.IsLetterOrDigit(span[i + 1]))
                {
                    nextNonLetterOrDigitShouldBeIgnored = true;
                    snakeCaseStringBuilder.Append(spaceReplacement);
                }
            }
        }

        return snakeCaseStringBuilder.ToString();
    }

    private static string SortLines(string text, ReadOnlyMemoryOfCharComparerDelegate lineComparer, EndOfLineSequence endOfLineSequence = EndOfLineSequence.Unknown)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Split text by lines.
        List<ReadOnlyMemory<char>> lines = text.AsMemory().ToLines();

        // Order lines.
        lines.Sort(new CustomReadOnlyMemoryOfCharComparer(lineComparer));

        // Rebuilt the text.
        return lines.BuildStringFromListOfLines(endOfLineSequence);
    }

    private static bool TextMatchAtIndex(string data, string test, int startIndex)
    {
        if (string.IsNullOrEmpty(test))
        {
            return false;
        }

        if (data.Length < test.Length)
        {
            return false;
        }

        for (int i = 0; i < test.Length; i++)
        {
            if (data[startIndex + i] != test[i])
            {
                return false;
            }
        }

        return true;
    }
}
