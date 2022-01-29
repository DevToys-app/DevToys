#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DevToys.Shared.Core;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal abstract class Formatter
    {
        private static readonly Regex SpacesEndRegex = new Regex(@"[ \t]+$", RegexOptions.Compiled);
        private static readonly Regex WhitespacesRegex = new Regex(@"\s+$", RegexOptions.Compiled);
        private static readonly Regex CommentWhitespacesRegex = new Regex(@"\n[ \t]*", RegexOptions.Compiled);

        private readonly InlineBlock _inlineBlock = new();

        private Indentation? _indentation = null;
        private SqlFormatterOptions _options;
        private Params? _params = null;
        private IReadOnlyList<Token>? _tokens = null;
        protected Token? _previousReservedToken = null;
        private int _index;

        /// <summary>
        /// Formats whitespace in a SQL string to make it easier to read.
        /// </summary>
        internal string Format(string query)
        {
            return Format(query, new SqlFormatterOptions(indentationSize: 2, uppercase: false));
        }

        /// <summary>
        /// Formats whitespace in a SQL string to make it easier to read.
        /// </summary>
        internal string Format(string query, SqlFormatterOptions options)
        {
            _options = options;
            _indentation = new Indentation(options.IndentationSize);
            _params = new Params(options.PlaceholderParameters);

            _tokens = GetTokenizer().Tokenize(query);
            string? formattedQuery = GetFormattedQueryFromTokens();
            return formattedQuery.Trim();
        }

        /// <summary>
        /// SQL Tokenizer for this formatter, provided by subclasses.
        /// </summary>
        protected abstract Tokenizer GetTokenizer();

        /// <summary>
        /// Reprocess and modify a token based on parsed context.
        /// </summary>
        protected virtual Token TokenOverride(Token token)
        {
            // subclasses can override this to modify tokens during formatting
            return token;
        }

        protected Token? TokenLookBehind(int n = 1)
        {
            if (_tokens is null || _tokens!.Count <= _index - n || _index - n < 0)
            {
                return null;
            }

            return _tokens![_index - n];
        }

        protected Token? TokenLookAhead(int n = 1)
        {
            if (_tokens is null || _tokens!.Count <= _index + n)
            {
                return null;
            }
            return _tokens![_index + n];
        }

        private string GetFormattedQueryFromTokens()
        {
            string formattedQuery = string.Empty;

            Assumes.NotNull(_tokens, nameof(_tokens));
            for (int i = 0; i < _tokens!.Count; i++)
            {
                _index = i;

                Token token = TokenOverride(_tokens[i]);
                if (token.Type == TokenType.LineComment)
                {
                    formattedQuery = FormatLineComment(token, formattedQuery);
                }
                else if (token.Type == TokenType.BlockComment)
                {
                    formattedQuery = FormatBlockComment(token, formattedQuery);
                }
                else if (token.Type == TokenType.ReservedTopLevel)
                {
                    formattedQuery = FormatTopLevelReservedWord(token, formattedQuery);
                    _previousReservedToken = token;
                }
                else if (token.Type == TokenType.ReservedTopLevelNoIndent)
                {
                    formattedQuery = FormatTopLevelReservedWordNoIndent(token, formattedQuery);
                    _previousReservedToken = token;
                }
                else if (token.Type == TokenType.ReservedNewLine)
                {
                    formattedQuery = FormatNewlineReservedWord(token, formattedQuery);
                    _previousReservedToken = token;
                }
                else if (token.Type == TokenType.Reserved)
                {
                    formattedQuery = FormatWithSpaces(token, formattedQuery);
                    _previousReservedToken = token;
                }
                else if (token.Type == TokenType.OpenParen)
                {
                    formattedQuery = FormatOpeningParentheses(token, formattedQuery);
                }
                else if (token.Type == TokenType.CloseParen)
                {
                    formattedQuery = FormatClosingParentheses(token, formattedQuery);
                }
                else if (token.Type == TokenType.PlaceHolder)
                {
                    formattedQuery = FormatPlaceholder(token, formattedQuery);
                }
                else if (string.Equals(token.Value, ",", System.StringComparison.Ordinal))
                {
                    formattedQuery = FormatComma(token, formattedQuery);
                }
                else if (string.Equals(token.Value, ":", System.StringComparison.Ordinal))
                {
                    formattedQuery = FormatWithSpaceAfter(token, formattedQuery);
                }
                else if (string.Equals(token.Value, ".", System.StringComparison.Ordinal))
                {
                    formattedQuery = FormatWithoutSpaces(token, formattedQuery);
                }
                else if (string.Equals(token.Value, ";", System.StringComparison.Ordinal))
                {
                    formattedQuery = FormatQuerySeparator(token, formattedQuery);
                }
                else
                {
                    formattedQuery = FormatWithSpaces(token, formattedQuery);
                }
            }

            return formattedQuery;
        }

        private string FormatLineComment(Token token, string query)
        {
            return AddNewLine(query + Show(token));
        }

        private string FormatBlockComment(Token token, string query)
        {
            return AddNewLine(AddNewLine(query) + IndentComment(token.Value));
        }

        private string IndentComment(string comment)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            return CommentWhitespacesRegex.Replace(comment, "\n" + _indentation!.GetIndent() + " ");
        }

        private string FormatTopLevelReservedWordNoIndent(Token token, string query)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            _indentation!.DecreaseTopLevel();

            query = AddNewLine(query) + EqualizeWhitespace(Show(token));
            return AddNewLine(query);
        }

        private string FormatTopLevelReservedWord(Token token, string query)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            _indentation!.DecreaseTopLevel();

            query = AddNewLine(query);

            _indentation.IncreaseTopLevel();

            query += EqualizeWhitespace(Show(token));
            return AddNewLine(query);
        }

        private string FormatNewlineReservedWord(Token token, string query)
        {
            if (TokenHelper.IsAnd(token) && TokenHelper.isBetween(TokenLookBehind(2)))
            {
                return FormatWithSpaces(token, query);
            }

            return AddNewLine(query) + EqualizeWhitespace(Show(token)) + " ";
        }

        /// <summary>
        /// Replace any sequence of whitespace characters with single space
        /// </summary>
        private string EqualizeWhitespace(string input)
        {
            return WhitespacesRegex.Replace(input, " ");
        }

        /// <summary>
        /// Opening parentheses increase the block indent level and start a new line
        /// </summary>
        private string FormatOpeningParentheses(Token token, string query)
        {
            // Take out the preceding space unless there was whitespace there in the original query
            // or another opening parens or line comment
            if (token.WhitespaceBefore?.Length == 0)
            {
                TokenType? type = TokenLookBehind()?.Type;
                if (type.HasValue && type != TokenType.OpenParen && type != TokenType.LineComment && type != TokenType.Operator)
                {
                    query = TrimSpacesEnd(query);
                }
            }

            query += Show(token);

            Assumes.NotNull(_tokens, nameof(token));
            _inlineBlock.BeginIfPossible(_tokens!, _index);

            if (!_inlineBlock.IsActive())
            {
                Assumes.NotNull(_indentation, nameof(_indentation));
                _indentation!.IncreaseBlockLevel();
                query = AddNewLine(query);
            }

            return query;
        }

        /// <summary>
        /// Closing parentheses decrease the block indent level
        /// </summary>
        private string FormatClosingParentheses(Token token, string query)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            if (_inlineBlock.IsActive())
            {
                _inlineBlock.End();
                return FormatWithSpaceAfter(token, query);
            }
            else
            {
                _indentation!.DecreaseBlockLevel();
                return FormatWithSpaces(token, AddNewLine(query));
            }
        }

        private string FormatPlaceholder(Token token, string query)
        {
            Assumes.NotNull(_params, nameof(_params));
            return query + _params!.Get(token) + ' ';
        }

        /// <summary>
        /// Commas start a new line (unless within inline parentheses or SQL "LIMIT" clause)
        /// </summary>
        private string FormatComma(Token token, string query)
        {
            query = FormatWithSpaceAfter(token, query);

            if (_inlineBlock.IsActive())
            {
                return query;
            }
            else if (TokenHelper.isLimit(_previousReservedToken))
            {
                return query;
            }
            else
            {
                return AddNewLine(query);
            }
        }

        private string FormatWithSpaceAfter(Token token, string query)
        {
            return TrimSpacesEnd(query) + Show(token) + " ";
        }

        private string FormatWithoutSpaces(Token token, string query)
        {
            return TrimSpacesEnd(query) + Show(token);
        }

        private string FormatWithSpaces(Token token, string query)
        {
            return query + Show(token) + " ";
        }

        private string FormatQuerySeparator(Token token, string query)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            _indentation!.ResetIndentation();
            return TrimSpacesEnd(query) + Show(token) + string.Concat(Enumerable.Repeat("\r\n", _options.LinesBetweenQueries));
        }

        /// <summary>
        /// Converts token to string (uppercasing it if needed)
        /// </summary>
        private string Show(Token token)
        {
            if (_options.Uppercase
                && (token.Type == TokenType.Reserved
                || token.Type == TokenType.ReservedTopLevel
                || token.Type == TokenType.ReservedTopLevelNoIndent
                || token.Type == TokenType.ReservedNewLine
                || token.Type == TokenType.OpenParen
                || token.Type == TokenType.CloseParen))
            {
                return token.Value.ToUpperInvariant();
            }
            else
            {
                return token.Value;
            }
        }

        private string AddNewLine(string query)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            query = TrimSpacesEnd(query);
            if (!query.EndsWith('\n'))
            {
                query += Environment.NewLine;
            }
            return query + _indentation!.GetIndent();
        }

        private string TrimSpacesEnd(string input)
        {
            return SpacesEndRegex.Replace(input, string.Empty);
        }
    }
}
