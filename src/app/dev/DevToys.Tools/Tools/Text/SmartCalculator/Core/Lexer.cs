using System.Collections;
using System.Collections.Immutable;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

[Export(typeof(ILexer))]
internal sealed class Lexer : ILexer
{
    private readonly IEnumerable<Lazy<IGrammarProvider, CultureCodeMetadata>> _grammarProviders;
    private readonly Dictionary<string, IReadOnlyList<TokenDefinition>> _tokenDefinitionGrammars = new();

    internal readonly string[] LineBreakers = new[] { "\r\n", "\n" };

    [ImportingConstructor]
    public Lexer(
        [ImportMany] IEnumerable<Lazy<IGrammarProvider, CultureCodeMetadata>> grammarProviders)
    {
        _grammarProviders = grammarProviders;
    }

    public IReadOnlyList<TokenizedTextLine> Tokenize(string culture, string? wholeDocument)
    {
        IReadOnlyList<TokenDefinition> orderedTokenDefinitionGrammars = GetTokenDefinitionGrammars(culture);

        var tokenizedLines = new List<TokenizedTextLine>();

        if (!string.IsNullOrEmpty(wholeDocument))
        {
            IReadOnlyList<string> lines = SplitLines(wholeDocument!);

            TokenizedTextLine? previousTokenizedLine = null;
            int i = 0;
            while (i < lines.Count)
            {
                TokenizedTextLine tokenizedLine = TokenizeLineInternal(lines[i], previousTokenizedLine, orderedTokenDefinitionGrammars);
                tokenizedLines.Add(tokenizedLine);

                previousTokenizedLine = tokenizedLine;
                i++;
            }
        }

        if (tokenizedLines.Count == 0)
        {
            tokenizedLines.Add(
                new TokenizedTextLine(
                    0,
                    0,
                    0,
                    string.Empty,
                    null));
        }

        return tokenizedLines;
    }

    /// <summary>
    /// Tokenize a line of text.
    /// </summary>
    /// <param name="startPositionInDocument">Location of the line in the document.</param>
    /// <param name="lineTextIncludingLineBreak">The whole text of the line, including the line break characters.</param>
    /// <param name="knownData">An ordered list of <see cref="IData"/> that have already been parsed in the <paramref name="lineTextIncludingLineBreak"/>.</param>
    /// <returns>A tokenized line.</returns>
    internal TokenizedTextLine TokenizeLine(
        string culture,
        int lineNumber,
        int startPositionInDocument,
        string lineTextIncludingLineBreak,
        IReadOnlyList<string> knownVariableNames,
        IReadOnlyList<IData>? knownData = null)
    {
        Guard.IsGreaterThanOrEqualTo(startPositionInDocument, 0);

        IReadOnlyList<TokenDefinition> tokenDefinitionGrammars = GetTokenDefinitionGrammars(culture);

        if (!string.IsNullOrEmpty(lineTextIncludingLineBreak))
            return TokenizeLineInternal(lineNumber, startPositionInDocument, lineTextIncludingLineBreak, tokenDefinitionGrammars, knownVariableNames, knownData);
        else
        {
            return new TokenizedTextLine(startPositionInDocument, lineNumber, 0, lineTextIncludingLineBreak, null);
        }
    }

    private TokenizedTextLine TokenizeLineInternal(
        string lineTextIncludingLineBreak,
        TokenizedTextLine? previousTokenizedLine,
        IReadOnlyList<TokenDefinition> orderedTokenDefinitionGrammars,
        IReadOnlyList<string>? orderedKnownVariableNames = null,
        IReadOnlyList<IData>? knownData = null)
    {
        int lineStart = previousTokenizedLine?.EndIncludingLineBreak ?? 0;
        int lineNumber = previousTokenizedLine?.LineNumber ?? -1;
        return TokenizeLineInternal(lineNumber + 1, lineStart, lineTextIncludingLineBreak, orderedTokenDefinitionGrammars, orderedKnownVariableNames, knownData);
    }

    private TokenizedTextLine TokenizeLineInternal(
        int lineNumber,
        int startPositionInDocument,
        string lineTextIncludingLineBreak,
        IReadOnlyList<TokenDefinition> orderedTokenDefinitionGrammars,
        IReadOnlyList<string>? orderedKnownVariableNames = null,
        IReadOnlyList<IData>? knownData = null)
    {
        Guard.IsGreaterThanOrEqualTo(startPositionInDocument, 0);

        var tokenEnumerator
            = new LineTokenEnumerator(
                lineTextIncludingLineBreak,
                orderedTokenDefinitionGrammars,
                orderedKnownVariableNames,
                knownData);
        // TODO: should we dispose this enumerator at some point?

        int lineBreakLength = 0;
        for (int i = 0; i < LineBreakers.Length; i++)
        {
            string lineBreaker = LineBreakers[i];
            if (lineTextIncludingLineBreak.EndsWith(lineBreaker, StringComparison.Ordinal))
            {
                lineBreakLength = lineBreaker.Length;
                break;
            }
        }

        LinkedToken? linkedToken = null;
        if (tokenEnumerator.MoveNext() && tokenEnumerator.Current is not null)
        {
            linkedToken
                = new LinkedToken(
                    previous: null,
                    token: tokenEnumerator.Current,
                    tokenEnumerator);
        }

        return new TokenizedTextLine(
            startPositionInDocument,
            lineNumber,
            lineBreakLength,
            lineTextIncludingLineBreak,
            linkedToken);
    }

    /// <summary>
    /// Split an <paramref name="input"/> per lines and keep the break line in the result.
    /// </summary>
    private IReadOnlyList<string> SplitLines(string input)
    {
        var lines = new List<string>() { input };

        for (int i = 0; i < LineBreakers.Length; i++)
        {
            string delimiter = LineBreakers[i];
            for (int j = 0; j < lines.Count; j++)
            {
                int index = lines[j].IndexOf(delimiter);
                if (index > -1
                    && lines[j].Length > index + 1)
                {
                    string leftPart = lines[j].Substring(0, index + delimiter.Length);
                    string rightPart = lines[j].Substring(index + delimiter.Length);
                    lines[j] = leftPart;
                    lines.Insert(j + 1, rightPart);
                }
            }
        }

        return lines;
    }

    private IReadOnlyList<TokenDefinition> GetTokenDefinitionGrammars(string culture)
    {
        lock (_tokenDefinitionGrammars)
        {
            string key = culture;
            if (_tokenDefinitionGrammars.TryGetValue(key, out IReadOnlyList<TokenDefinition>? tokenDefinitions) && tokenDefinitions is not null)
                return tokenDefinitions;

            IEnumerable<TokenDefinitionGrammar> grammars
                = _grammarProviders.Where(
                    p => p.Metadata.CultureCodes.Any(
                        c => CultureHelper.IsCultureApplicable(c, culture)))
                .Select(p => p.Value.LoadTokenDefinitionGrammars(culture))
                .Where(g => g is not null)
                .SelectMany(g => g!)!;

            var definitions = new List<TokenDefinition>();

            foreach (TokenDefinitionGrammar grammar in grammars)
            {
                if (grammar.CommonTokens is not null)
                {
                    foreach (KeyValuePair<string, string[]> item in grammar.CommonTokens)
                    {
                        foreach (string identifier in item.Value)
                        {
                            definitions.Add(new TokenDefinition { TokenText = identifier, TokenType = item.Key });
                        }
                    }
                }
            }

            // Order tokens from the longest to the shortest.
            tokenDefinitions = definitions.ToImmutableSortedSet(new DescendingComparer<TokenDefinition>());
            _tokenDefinitionGrammars[key] = tokenDefinitions;
            return tokenDefinitions;
        }
    }

    private record TokenDefinition : IComparable<TokenDefinition>
    {
        public string TokenText { get; set; } = default!;

        public string TokenType { get; set; } = default!;

        public int CompareTo(TokenDefinition? other)
        {
            int result = TokenText.Length.CompareTo(other!.TokenText.Length);
            if (result == 0)
                result = TokenText.CompareTo(other.TokenText);
            return result;
        }
    }

    private class LineTokenEnumerator : ITokenEnumerator
    {
        private readonly object _syncLock = new();
        private readonly string _lineTextIncludingLineBreak;
        private readonly IReadOnlyList<TokenDefinition> _orderedTokenDefinitionGrammars;
        private readonly IReadOnlyList<string>? _orderedKnownVariableNames;
        private readonly IReadOnlyList<IData>? _knownData;

        private bool _disposed;
        private int _currentPositionInLine;
        private IToken? _currentToken;

        public IToken? Current
        {
            get
            {
                lock (_syncLock)
                {
                    ThrowIfDisposed();
                    return _currentToken;
                }
            }
        }

        object? IEnumerator.Current => Current;

        public LineTokenEnumerator(
            string lineTextIncludingLineBreak,
            IReadOnlyList<TokenDefinition> orderedTokenDefinitionGrammars,
            IReadOnlyList<string>? orderedKnownVariableNames,
            IReadOnlyList<IData>? knownData)
        {
            Guard.IsNotNull(lineTextIncludingLineBreak);
            _lineTextIncludingLineBreak = lineTextIncludingLineBreak;
            _orderedTokenDefinitionGrammars = orderedTokenDefinitionGrammars;
            _orderedKnownVariableNames = orderedKnownVariableNames;
            _knownData = knownData;
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                _disposed = true;
            }
        }

        public bool MoveNext()
        {
            lock (_syncLock)
            {
                ThrowIfDisposed();

                if (string.IsNullOrEmpty(_lineTextIncludingLineBreak)
                    || _currentPositionInLine >= _lineTextIncludingLineBreak.Length)
                {
                    _currentToken = null;
                    return false;
                }

                IToken? token = DetectToken(_currentPositionInLine);

                if (token is null || token.IsOfType(PredefinedTokenAndDataTypeNames.NewLine))
                {
                    _currentToken = null;
                    return false;
                }

                _currentToken = token;
                _currentPositionInLine = token.EndInLine;
                return true;
            }
        }

        public void Reset()
        {
            lock (_syncLock)
            {
                ThrowIfDisposed();
                _currentPositionInLine = 0;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                ThrowHelper.ThrowObjectDisposedException(nameof(LineTokenEnumerator));
        }

        private IToken? DetectToken(int startIndex)
        {
            while (_lineTextIncludingLineBreak.Length > startIndex)
            {
                if (TryFindDataAtPosition(startIndex, out IData? foundData) && foundData is not null)
                    return foundData;
                else if (TryDetectTokenFromGrammarAtPosition(startIndex, out Token? foundToken) && foundToken is not null)
                {
                    return foundToken;
                }

                char startChar = _lineTextIncludingLineBreak[startIndex];
                int endIndex = startIndex + 1;

                string tokenType = DetectTokenType(startChar);

                if (tokenType != PredefinedTokenAndDataTypeNames.Whitespace)
                {
                    if (_lineTextIncludingLineBreak.Length > startIndex)
                    {
                        int nextCharIndex;
                        switch (tokenType)
                        {
                            case PredefinedTokenAndDataTypeNames.Word:
                            case PredefinedTokenAndDataTypeNames.Digit:
                                nextCharIndex = GetEndPositionOfRepeatedTokenType(startIndex, tokenType);
                                if (nextCharIndex > startIndex + 1)
                                    endIndex = nextCharIndex;
                                break;

                            case PredefinedTokenAndDataTypeNames.NewLine:
                                if (startChar == '\r')
                                {
                                    nextCharIndex = startIndex + 1;
                                    if (_lineTextIncludingLineBreak.Length > nextCharIndex)
                                    {
                                        char nextChar = _lineTextIncludingLineBreak[nextCharIndex];
                                        if (nextChar == '\n')
                                            endIndex = nextCharIndex + 1;
                                    }
                                }
                                break;

                            case PredefinedTokenAndDataTypeNames.SymbolOrPunctuation:
                            case PredefinedTokenAndDataTypeNames.LeftParenth:
                            case PredefinedTokenAndDataTypeNames.RightParenth:
                                break;

                            default:
                                ThrowHelper.ThrowNotSupportedException();
                                break;
                        }
                    }

                    return new Token(_lineTextIncludingLineBreak, startIndex, endIndex, tokenType);
                }

                startIndex++;
            }

            return null;
        }

        private bool TryFindDataAtPosition(int startIndex, out IData? foundData)
        {
            if (_knownData is not null)
            {
                // Use binary search to find the data at a given position in the line.
                int min = 0;
                int max = _knownData.Count - 1;
                while (min <= max)
                {
                    int middle = (min + max) / 2;
                    IData currentData = _knownData[middle];
                    if (startIndex == currentData.StartInLine)
                    {
                        foundData = currentData;
                        return true;
                    }
                    else if (startIndex < currentData.StartInLine)
                    {
                        max = middle - 1;
                    }
                    else
                    {
                        min = middle + 1;
                    }
                }
            }

            foundData = null;
            return false;
        }

        private bool TryDetectTokenFromGrammarAtPosition(int startIndex, out Token? foundToken)
        {
            // Detect tokens.
            if (_orderedTokenDefinitionGrammars is not null)
            {
                for (int i = 0; i < _orderedTokenDefinitionGrammars.Count; i++)
                {
                    TokenDefinition tokenDefinition = _orderedTokenDefinitionGrammars[i];
                    if (_lineTextIncludingLineBreak.Length >= startIndex + tokenDefinition.TokenText.Length)
                    {
                        if (_lineTextIncludingLineBreak.IndexOf(tokenDefinition.TokenText, startIndex, tokenDefinition.TokenText.Length, StringComparison.OrdinalIgnoreCase) == startIndex)
                        {
                            bool tokenFound = true;
                            string lastTokenCharacterType = DetectTokenType(tokenDefinition.TokenText[tokenDefinition.TokenText.Length - 1]);
                            if (lastTokenCharacterType == PredefinedTokenAndDataTypeNames.Word)
                            {
                                int nextCharIndex = GetEndPositionOfRepeatedTokenType(startIndex, lastTokenCharacterType);
                                if (nextCharIndex > startIndex + tokenDefinition.TokenText.Length)
                                    tokenFound = false;
                            }

                            if (tokenFound)
                            {
                                foundToken
                                    = new Token(
                                        _lineTextIncludingLineBreak,
                                        startIndex,
                                        startIndex + tokenDefinition.TokenText.Length,
                                        tokenDefinition.TokenType);
                                return true;
                            }
                        }
                    }
                }
            }

            // Detect variables.
            if (_orderedKnownVariableNames is not null)
            {
                for (int i = 0; i < _orderedKnownVariableNames.Count; i++)
                {
                    string variableName = _orderedKnownVariableNames[i];
                    if (_lineTextIncludingLineBreak.Length >= startIndex + variableName.Length
                        && _lineTextIncludingLineBreak.IndexOf(variableName, startIndex, variableName.Length, StringComparison.Ordinal) == startIndex)
                    {
                        bool tokenFound = true;
                        string lastTokenCharacterType = DetectTokenType(variableName[variableName.Length - 1]);
                        if (lastTokenCharacterType == PredefinedTokenAndDataTypeNames.Word)
                        {
                            int nextCharIndex = GetEndPositionOfRepeatedTokenType(startIndex, lastTokenCharacterType);
                            if (nextCharIndex > startIndex + variableName.Length)
                                tokenFound = false;
                        }

                        if (tokenFound)
                        {
                            foundToken
                            = new Token(
                                _lineTextIncludingLineBreak,
                                startIndex,
                                startIndex + variableName.Length,
                                PredefinedTokenAndDataTypeNames.VariableReference);
                            return true;
                        }
                    }
                }
            }

            foundToken = null;
            return false;
        }

        private int GetEndPositionOfRepeatedTokenType(int startIndex, string tokenType)
        {
            int nextCharIndex = startIndex;
            do
            {
                nextCharIndex++;
            } while (_lineTextIncludingLineBreak.Length > nextCharIndex && DetectTokenType(_lineTextIncludingLineBreak[nextCharIndex]) == tokenType);
            return nextCharIndex;
        }

        private static string DetectTokenType(char c)
        {
            if (c == '\r' || c == '\n')
                return PredefinedTokenAndDataTypeNames.NewLine;

            if (char.IsDigit(c))
                return PredefinedTokenAndDataTypeNames.Digit;

            if (char.IsWhiteSpace(c))
                return PredefinedTokenAndDataTypeNames.Whitespace;

            if (c == '(')
                return PredefinedTokenAndDataTypeNames.LeftParenth;

            if (c == ')')
                return PredefinedTokenAndDataTypeNames.RightParenth;

            if (char.IsPunctuation(c)
                || char.IsSymbol(c)
                || c == 'π'
                || c == '¾'
                || c == '½'
                || c == '¼'
                || c == 'º'
                || c == '¹'
                || c == '²'
                || c == '³'
                || c == 'µ'
                || c == '­'
                || c == 'ª')
            {
                return PredefinedTokenAndDataTypeNames.SymbolOrPunctuation;
            }

            if (char.IsLetter(c))
                return PredefinedTokenAndDataTypeNames.Word;

            return PredefinedTokenAndDataTypeNames.UnsupportedCharacter;
        }
    }
}
