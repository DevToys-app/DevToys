#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal class Tokenizer
    {
        private static readonly Regex WhitespaceRegex = new Regex(@"^(\s+)", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new Regex(@"^((-\s*)?[0-9]+(\.[0-9]+)?([eE]-?[0-9]+(\.[0-9]+)?)?|0x[0-9a-fA-F]+|0b[01]+)\b", RegexOptions.Compiled);
        private static readonly Regex BlockCommentRegex = new Regex(@"^(\/\*(.*?)*?(?:\*\/|$))", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly Regex _operatorRegex;
        private readonly Regex _lineCommentRegex;
        private readonly Regex _reservedTopLevelRegex;
        private readonly Regex _reservedTopLevelNoIndentRegex;
        private readonly Regex _reservedNewLineRegex;
        private readonly Regex _reservedPlainRegex;
        private readonly Regex _wordRegex;
        private readonly Regex _stringRegex;
        private readonly Regex _openParenRegex;
        private readonly Regex _closeParenRegex;
        private readonly Regex? _indexedPlaceholderRegex;
        private readonly Regex? _indentNamedPlaceholderRegex;
        private readonly Regex? _stringNamedPlaceholderRegex;

        /// <summary>
        /// Initializes a new instance of <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="reservedWords">Reserved words in SQL</param>
        /// <param name="reservedTopLevelWords">Words that are set to new line separately</param>
        /// <param name="reservedNewlineWords">Words that are set to newline</param>
        /// <param name="reservedTopLevelWordsNoIndent">Words that are top level but have no indentation</param>
        /// <param name="stringTypes">String types to enable: "", '', ``, [], N''</param>
        /// <param name="openParens">Opening parentheses to enable, like (, [</param>
        /// <param name="closeParens">Closing parentheses to enable, like ), ]</param>
        /// <param name="indexedPlaceholderTypes">Prefixes for indexed placeholders, like ?</param>
        /// <param name="namedPlaceholderTypes">Prefixes for named placeholders, like @ and :</param>
        /// <param name="lineCommentTypes">Line comments to enable, like # and --</param>
        /// <param name="specialWordChars">Special chars that can be found inside of words, like @ and #</param>
        /// <param name="operators">Additional operators to recognize</param>
        public Tokenizer(
            string[] reservedWords,
            string[] reservedTopLevelWords,
            string[] reservedNewlineWords,
            string[] reservedTopLevelWordsNoIndent,
            string[] stringTypes,
            string[] openParens,
            string[] closeParens,
            string[] indexedPlaceholderTypes,
            string[] namedPlaceholderTypes,
            string[] lineCommentTypes,
            string[] specialWordChars,
            string[]? operators = null)
        {
            var operatorsParam = new List<string> { "<>", "<=", ">=" };
            if (operators is not null)
            {
                operatorsParam.AddRange(operators);
            }
            _operatorRegex = RegexFactory.CreateOperatorRegex(operatorsParam);

            _lineCommentRegex = RegexFactory.CreateLineCommentRegex(lineCommentTypes);
            _reservedTopLevelRegex = RegexFactory.CreateReservedWordRegex(reservedTopLevelWords);
            _reservedTopLevelNoIndentRegex = RegexFactory.CreateReservedWordRegex(reservedTopLevelWordsNoIndent);
            _reservedNewLineRegex = RegexFactory.CreateReservedWordRegex(reservedNewlineWords);
            _reservedPlainRegex = RegexFactory.CreateReservedWordRegex(reservedWords);
            _wordRegex = RegexFactory.CreateWordRegex(specialWordChars);
            _stringRegex = RegexFactory.CreateStringRegex(stringTypes);
            _openParenRegex = RegexFactory.CreateParenRegex(openParens);
            _closeParenRegex = RegexFactory.CreateParenRegex(closeParens);
            _indexedPlaceholderRegex = RegexFactory.CreatePlaceholderRegex(indexedPlaceholderTypes, "[0-9]*");
            _indentNamedPlaceholderRegex = RegexFactory.CreatePlaceholderRegex(namedPlaceholderTypes, "[a-zA-Z0-9._$]+");
            _stringNamedPlaceholderRegex = RegexFactory.CreatePlaceholderRegex(namedPlaceholderTypes, RegexFactory.CreateStringPattern(stringTypes));
        }

        /// <summary>
        /// Takes a SQL string and breaks it into tokens.
        /// </summary>
        /// <param name="input">The SQL string</param>
        /// <returns></returns>
        internal IReadOnlyList<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            Token? token = null;

            // Keep processing the string until it is empty
            while (input.Length > 0)
            {
                // TODO: This is a direct translation from https://github.com/zeroturnaround/sql-formatter
                // The current algorithm allocates a lot of strings, which is terrible from memory consumption perspective.

                // grab any preceding whitespace
                string whitespaceBefore = GetWhitespace(input);
                if (whitespaceBefore.Length > 0)
                {
                    input = input.Substring(whitespaceBefore.Length);
                }

                if (input.Length > 0)
                {
                    // Get the next token and the token type
                    token = GetNextToken(input, token);

                    if (token is not null)
                    {
                        // Advance the string
                        input = input.Substring(token.Value.Length);
                        token.WhitespaceBefore = whitespaceBefore;
                        tokens.Add(token);
                    }
                }
            }

            return tokens;
        }

        private string GetWhitespace(string input)
        {
            MatchCollection? matches = WhitespaceRegex.Matches(input);
            return matches is null || matches.Count == 0 ? string.Empty : matches[0].Value;
        }

        private Token? GetNextToken(string input, Token? previousToken)
        {
            return GetCommentToken(input)
                ?? GetStringToken(input)
                ?? GetOpenParenToken(input)
                ?? GetCloseParenToken(input)
                ?? GetPlaceholderToken(input)
                ?? GetNumberToken(input)
                ?? GetReservedWordToken(input, previousToken)
                ?? GetWordToken(input)
                ?? GetOperatorToken(input);
        }

        private Token? GetCommentToken(string input)
        {
            return GetLineCommentToken(input)
                ?? GetBlockCommentToken(input);
        }

        private Token? GetLineCommentToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.LineComment, _lineCommentRegex);
        }

        private Token? GetBlockCommentToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.BlockComment, BlockCommentRegex);
        }

        private Token? GetStringToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.String, _stringRegex);
        }

        private Token? GetOpenParenToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.OpenParen, _openParenRegex);
        }

        private Token? GetCloseParenToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.CloseParen, _closeParenRegex);
        }

        private Token? GetPlaceholderToken(string input)
        {
            return GetIdentNamedPlaceholderToken(input)
                ?? GetStringNamedPlaceholderToken(input)
                ?? GetIndexedPlaceholderToken(input);
        }

        private Token? GetIdentNamedPlaceholderToken(string input)
        {
            return GetPlaceholderTokenWithKey(input, _indentNamedPlaceholderRegex, (v) => v.Substring(1));
        }

        private Token? GetStringNamedPlaceholderToken(string input)
        {
            return GetPlaceholderTokenWithKey(
                input,
                _stringNamedPlaceholderRegex,
                (v) => GetEscapedPlaceholderKey(
                    key: v.Substring(1, 1),
                    quoteChar: v.Substring(v.Length - 2)));
        }

        private Token? GetIndexedPlaceholderToken(string input)
        {
            return GetPlaceholderTokenWithKey(input, _indexedPlaceholderRegex, (v) => v.Substring(1));
        }

        private Token? GetPlaceholderTokenWithKey(string input, Regex? regex, Func<string, string> parseKey)
        {
            Token? token = GetTokenOnFirstMatch(input, TokenType.PlaceHolder, regex);
            if (token is not null)
            {
                token.Key = parseKey(token.Value);
            }

            return token;
        }

        private string GetEscapedPlaceholderKey(string key, string quoteChar)
        {
            string? escapeRegexPattern = new Regex(@"[.*+?^${}()|[\]\\]").Replace("\\" + quoteChar, "\\$&");
            var escapeRegex = new Regex(escapeRegexPattern);
            return escapeRegex.Replace(key, quoteChar);
        }

        private Token? GetNumberToken(string input)
        {
            // Decimal, binary, or hex numbers
            return GetTokenOnFirstMatch(input, TokenType.Number, NumberRegex);
        }

        private Token? GetWordToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.Word, _wordRegex);
        }

        private Token? GetOperatorToken(string input)
        {
            // Punctuation and symbols
            return GetTokenOnFirstMatch(input, TokenType.Operator, _operatorRegex);
        }

        private Token? GetReservedWordToken(string input, Token? previousToken)
        {
            // A reserved word cannot be preceded by a "."
            // this makes it so in "mytable.from", "from" is not considered a reserved word
            if (previousToken is not null && string.Equals(".", previousToken.Value))
            {
                return null;
            }

            return GetTopLevelReservedToken(input)
                ?? GetNewlineReservedToken(input)
                ?? GetTopLevelReservedTokenNoIndent(input)
                ?? GetPlainReservedToken(input);
        }

        private Token? GetTopLevelReservedToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.ReservedTopLevel, _reservedTopLevelRegex);
        }

        private Token? GetNewlineReservedToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.ReservedNewLine, _reservedNewLineRegex);
        }

        private Token? GetTopLevelReservedTokenNoIndent(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.ReservedTopLevelNoIndent, _reservedTopLevelNoIndentRegex);
        }

        private Token? GetPlainReservedToken(string input)
        {
            return GetTokenOnFirstMatch(input, TokenType.Reserved, _reservedPlainRegex);
        }

        private Token? GetTokenOnFirstMatch(string input, TokenType type, Regex? regex)
        {
            if (regex is null)
            {
                return null;
            }

            MatchCollection? matches = regex.Matches(input);
            if (matches is null || matches.Count == 0)
            {
                return null;
            }

            return new Token(matches[0].Value, type);
        }
    }
}
