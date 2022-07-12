#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal class Tokenizer
    {
        private static readonly Regex NumberRegex = new Regex(@"^((-\s*)?[0-9]+(\.[0-9]+)?([eE]-?[0-9]+(\.[0-9]+)?)?|0x[0-9a-fA-F]+|0b[01]+)\b", RegexOptions.Compiled, RegexFactory.DefaultMatchTimeout);
        private static readonly Regex BlockCommentRegex = new Regex(@"^(\/\*(.*?)*?(?:\*\/|$))", RegexOptions.Singleline | RegexOptions.Compiled, RegexFactory.DefaultMatchTimeout);

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
            char[] indexedPlaceholderTypes,
            char[] namedPlaceholderTypes,
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
            int pointerIndex = 0;

            // Keep processing the string until it is empty
            while (pointerIndex != input.Length)
            {
                // grab any preceding whitespace length
                int precedingWitespaceLenght = GetPrecedingWitespaceLenght(input, pointerIndex);

                pointerIndex += precedingWitespaceLenght;

                if (pointerIndex != input.Length)
                {
                    // Get the next token and the token type
                    token = GetNextToken(input, pointerIndex, previousToken: token);

                    if (token is not null)
                    {
                        Token t = token.Value;
                        // Advance the index pointer string
                        pointerIndex += t.Length;
                        t.PrecedingWitespaceLength = precedingWitespaceLenght;
                        tokens.Add(t);
                    }
                }
            }

            return tokens;
        }

        private int GetPrecedingWitespaceLenght(string input, int pointerIndex)
        {
            int i = 0;
            int len = input.Length - pointerIndex;
            for (; i < len; i++)
            {
                if (!char.IsWhiteSpace(input[i + pointerIndex]))
                {
                    break;
                }
            }
            return i;
        }

        private Token? GetNextToken(string input, int pointerIndex, Token? previousToken)
        {
            return GetCommentToken(input, pointerIndex)
                ?? GetStringToken(input, pointerIndex)
                ?? GetOpenParenToken(input, pointerIndex)
                ?? GetCloseParenToken(input, pointerIndex)
                ?? GetPlaceholderToken(input, pointerIndex)
                ?? GetNumberToken(input, pointerIndex)
                ?? GetReservedWordToken(input, pointerIndex, previousToken)
                ?? GetWordToken(input, pointerIndex)
                ?? GetOperatorToken(input, pointerIndex);
        }

        private Token? GetCommentToken(string input, int pointerIndex)
        {
            return GetLineCommentToken(input, pointerIndex)
                ?? GetBlockCommentToken(input, pointerIndex);
        }

        private Token? GetLineCommentToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.LineComment, _lineCommentRegex);
        }

        private Token? GetBlockCommentToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.BlockComment, BlockCommentRegex);
        }

        private Token? GetStringToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.String, _stringRegex);
        }

        private Token? GetOpenParenToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.OpenParen, _openParenRegex);
        }

        private Token? GetCloseParenToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.CloseParen, _closeParenRegex);
        }

        private Token? GetPlaceholderToken(string input, int pointerIndex)
        {
            return GetIdentNamedPlaceholderToken(input, pointerIndex)
                ?? GetIndexedPlaceholderToken(input, pointerIndex);
        }

        private Token? GetIdentNamedPlaceholderToken(string input, int pointerIndex)
        {
            return GetPlaceholderTokenWithKey(input, pointerIndex, _indentNamedPlaceholderRegex);
        }

        private Token? GetIndexedPlaceholderToken(string input, int pointerIndex)
        {
            return GetPlaceholderTokenWithKey(input, pointerIndex, _indexedPlaceholderRegex);
        }

        private Token? GetPlaceholderTokenWithKey(string input, int pointerIndex, Regex? regex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.PlaceHolder, regex);
        }

        private Token? GetNumberToken(string input, int pointerIndex)
        {
            // Decimal, binary, or hex numbers
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.Number, NumberRegex);
        }

        private Token? GetWordToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.Word, _wordRegex);
        }

        private Token? GetOperatorToken(string input, int pointerIndex)
        {
            // Punctuation and symbols
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.Operator, _operatorRegex);
        }

        private Token? GetReservedWordToken(string input, int pointerIndex, Token? previousToken)
        {
            // A reserved word cannot be preceded by a "."
            // this makes it so in "mytable.from", "from" is not considered a reserved word
            if (previousToken is not null 
                && previousToken.Value.Length == 1 
                && input[previousToken.Value.Index] == '.')
            {
                return null;
            }

            return GetTopLevelReservedToken(input, pointerIndex)
                ?? GetNewlineReservedToken(input, pointerIndex)
                ?? GetTopLevelReservedTokenNoIndent(input, pointerIndex)
                ?? GetPlainReservedToken(input, pointerIndex);
        }

        private Token? GetTopLevelReservedToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.ReservedTopLevel, _reservedTopLevelRegex);
        }

        private Token? GetNewlineReservedToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.ReservedNewLine, _reservedNewLineRegex);
        }

        private Token? GetTopLevelReservedTokenNoIndent(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.ReservedTopLevelNoIndent, _reservedTopLevelNoIndentRegex);
        }

        private Token? GetPlainReservedToken(string input, int pointerIndex)
        {
            return GetTokenOnFirstMatch(input, pointerIndex, TokenType.Reserved, _reservedPlainRegex);
        }

        private Token? GetTokenOnFirstMatch(string input, int pointerIndex, TokenType type, Regex? regex)
        {
            if (regex is null)
            {
                return null;
            }

            Match match = regex.Match(input, pointerIndex, input.Length - pointerIndex);

            return match.Success ? new Token(pointerIndex, match.Length, type) : null;
        }
    }
}
