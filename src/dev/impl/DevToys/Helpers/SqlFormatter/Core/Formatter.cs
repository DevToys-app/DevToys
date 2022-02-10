#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DevToys.Shared.Core;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal abstract class Formatter
    {
        private static readonly Regex WhitespacesRegex = new Regex(@"\s+$");
        private static readonly Regex CommentWhitespacesRegex = new Regex(@"\n[ \t]*");

        private readonly InlineBlock _inlineBlock = new();
        private readonly StringBuilder _queryBuilder = new();

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
            SetFormattedQueryFromTokens(query.AsSpan());
            return _queryBuilder.ToString().Trim();
        }

        /// <summary>
        /// SQL Tokenizer for this formatter, provided by subclasses.
        /// </summary>
        protected abstract Tokenizer GetTokenizer();

        /// <summary>
        /// Reprocess and modify a token based on parsed context.
        /// </summary>
        protected virtual Token TokenOverride(Token token, ReadOnlySpan<char> querySpan)
        {
            // subclasses can override this to modify tokens during formatting
            return token;
        }

        protected Token? TokenLookBehind(int n = 1)
        {
            return _tokens?.ElementAtOrDefault(_index - n);
        }

        protected Token? TokenLookAhead(int n = 1)
        {
            if (_tokens is null || _tokens!.Count <= _index + n)
            {
                return null;
            }
            return _tokens![_index + n];
        }

        private void SetFormattedQueryFromTokens(ReadOnlySpan<char> querySpan)
        {
            _queryBuilder.Clear();

            Assumes.NotNull(_tokens, nameof(_tokens));
            for (int i = 0; i < _tokens!.Count; i++)
            {
                _index = i;

                Token token = TokenOverride(_tokens[i], querySpan);
                switch (token.Type)
                {
                    case TokenType.LineComment:
                        FormatLineComment(token, querySpan);
                        break;
                    case TokenType.BlockComment:
                        FormatBlockComment(token, querySpan);
                        break;
                    case TokenType.ReservedTopLevel:
                        FormatTopLevelReservedWord(token, querySpan);
                        _previousReservedToken = token;
                        break;
                    case TokenType.ReservedTopLevelNoIndent:
                        FormatTopLvoidReservedWordNoIndent(token, querySpan);
                        _previousReservedToken = token;
                        break;
                    case TokenType.ReservedNewLine:
                        FormatNewlineReservedWord(token, querySpan);
                        _previousReservedToken = token;
                        break;
                    case TokenType.Reserved:
                        FormatWithSpaces(token, querySpan);
                        _previousReservedToken = token;
                        break;
                    case TokenType.OpenParen:
                        FormatOpeningParentheses(token, querySpan);
                        break;
                    case TokenType.CloseParen:
                        FormatClosingParentheses(token, querySpan);
                        break;
                    case TokenType.PlaceHolder:
                        FormatPlaceholder(token, querySpan);
                        break;
                    default:
                        switch (token.Length)
                        {
                            case 1 when querySpan[token.Index] == ',':
                                FormatComma(token, querySpan);
                                break;
                            case 1 when querySpan[token.Index] == ':':
                                FormatWithSpaceAfter(token, querySpan);
                                break;
                            case 1 when querySpan[token.Index] == '.':
                                FormatWithoutSpaces(token, querySpan);
                                break;
                            case 1 when querySpan[token.Index] == ';':
                                FormatQuerySeparator(token, querySpan);
                                break;
                            default:
                                FormatWithSpaces(token, querySpan);
                                break;
                        }
                        break;
                }
            }
        }

        private void FormatLineComment(Token token, ReadOnlySpan<char> querySpan)
        {
            _queryBuilder.Append(Show(querySpan.Slice(token), token.Type));
            AddNewLine();
        }

        private void FormatBlockComment(Token token, ReadOnlySpan<char> querySpan)
        {
            AddNewLine();
            _queryBuilder.Append(IndentComment(querySpan.Slice(token)));
            AddNewLine();
        }

        private string IndentComment(ReadOnlySpan<char> comment)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            return CommentWhitespacesRegex.Replace(comment.ToString(), $"\n{_indentation!.GetIndent()} ");
        }

        private void FormatTopLvoidReservedWordNoIndent(Token token, ReadOnlySpan<char> querySpan)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            _indentation!.DecreaseTopLevel();

            AddNewLine();

            _queryBuilder.Append(EqualizeWhitespace(Show(querySpan.Slice(token), token.Type)));

            AddNewLine();
        }

        private void FormatTopLevelReservedWord(Token token, ReadOnlySpan<char> querySpan)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            _indentation!.DecreaseTopLevel();

            AddNewLine();

            _indentation.IncreaseTopLevel();

            _queryBuilder.Append(EqualizeWhitespace(Show(querySpan.Slice(token), token.Type)));

            AddNewLine();
        }

        private void FormatNewlineReservedWord(Token token, ReadOnlySpan<char> querySpan)
        {
            if (token.IsAnd(querySpan.Slice(token)))
            {
                Token? t = TokenLookBehind(2);

                if (t != null && t.Value.isBetween(querySpan.Slice(t.Value)))
                {
                    FormatWithSpaces(token, querySpan);
                    return;
                }
            }
            AddNewLine();

            _queryBuilder.Append(EqualizeWhitespace(Show(querySpan.Slice(token), token.Type)));

            _queryBuilder.Append(' ');
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
        private void FormatOpeningParentheses(Token token, ReadOnlySpan<char> querySpan)
        {
            // Take out the preceding space unless there was whitespace there in the original query
            // or another opening parens or line comment
            if (token.PrecedingWitespaceLength == 0)
            {
                Token? behindToken = TokenLookBehind();

                if (behindToken is not { Type: TokenType.OpenParen or TokenType.LineComment or TokenType.Operator })
                {
                    _queryBuilder.TrimSpaceEnd();
                }
            }
            _queryBuilder.Append(Show(querySpan.Slice(token), token.Type));

            Assumes.NotNull(_tokens, nameof(token));
            _inlineBlock.BeginIfPossible(_tokens!, _index, querySpan.Slice(token));

            if (!_inlineBlock.IsActive())
            {
                Assumes.NotNull(_indentation, nameof(_indentation));
                _indentation!.IncreaseBlockLevel();
                AddNewLine();
            }
        }

        /// <summary>
        /// Closing parentheses decrease the block indent level
        /// </summary>
        private void FormatClosingParentheses(Token token, ReadOnlySpan<char> querySpan)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            if (_inlineBlock.IsActive())
            {
                _inlineBlock.End();
                FormatWithSpaceAfter(token, querySpan);
            }
            else
            {
                _indentation!.DecreaseBlockLevel();
                AddNewLine();
                FormatWithSpaces(token, querySpan);
            }
        }

        private void FormatPlaceholder(Token token, ReadOnlySpan<char> querySpan)
        {
            Assumes.NotNull(_params, nameof(_params));

            string? value = null;
            ReadOnlySpan<char> valueSpan = querySpan.Slice(token);
            if (valueSpan.Length != 0)
            {
                // assumme the length of all placeholder is 1, since only char array is accepted 
                value = _params!.Get(valueSpan.Slice(0, 1).ToString());
            }

            _queryBuilder.Append(value ?? querySpan.Slice(token).ToString());

            _queryBuilder.Append(' ');
        }

        /// <summary>
        /// Commas start a new line (unless within inline parentheses or SQL "LIMIT" clause)
        /// </summary>
        private void FormatComma(Token token, ReadOnlySpan<char> querySpan)
        {
            FormatWithSpaceAfter(token, querySpan);

            if (_inlineBlock.IsActive())
            {
                return;
            }
            else if (_previousReservedToken is not null
                && _previousReservedToken.Value.isLimit(querySpan.Slice(_previousReservedToken.Value)))
            {
                return;
            }

            AddNewLine();

        }

        private void FormatWithSpaceAfter(Token token, ReadOnlySpan<char> querySpan)
        {
            _queryBuilder.TrimSpaceEnd();
            _queryBuilder.Append(Show(querySpan.Slice(token), token.Type));
            _queryBuilder.Append(' ');
        }

        private void FormatWithoutSpaces(Token token, ReadOnlySpan<char> querySpan)
        {
            _queryBuilder.TrimSpaceEnd();
            _queryBuilder.Append(Show(querySpan.Slice(token), token.Type));
        }

        private void FormatWithSpaces(Token token, ReadOnlySpan<char> querySpan)
        {
            _queryBuilder.Append(Show(querySpan.Slice(token), token.Type));
            _queryBuilder.Append(' ');
        }

        private void FormatQuerySeparator(Token token, ReadOnlySpan<char> querySpan)
        {
            Assumes.NotNull(_indentation, nameof(_indentation));
            _indentation!.ResetIndentation();

            _queryBuilder.TrimSpaceEnd();
            _queryBuilder.Append(Show(querySpan.Slice(token), token.Type));

            int times = _options.LinesBetweenQueries;

            while (times != 0)
            {
                _queryBuilder.AppendLine();
                times--;
            }
        }

        /// <summary>
        /// Converts token to string (uppercasing it if needed)
        /// </summary>
        private string Show(ReadOnlySpan<char> value, TokenType tokenType)
        {
            if (_options.Uppercase
                && (tokenType == TokenType.Reserved
                || tokenType == TokenType.ReservedTopLevel
                || tokenType == TokenType.ReservedTopLevelNoIndent
                || tokenType == TokenType.ReservedNewLine
                || tokenType == TokenType.OpenParen
                || tokenType == TokenType.CloseParen))
            {
                return value.ToString().ToUpper();
            }
            else
            {
                return value.ToString();
            }
        }

        private void AddNewLine()
        {
            Assumes.NotNull(_indentation, nameof(_indentation));

            _queryBuilder.TrimSpaceEnd();

            if (_queryBuilder.Length != 0 && _queryBuilder[_queryBuilder.Length - 1] != '\n')
            {
                _queryBuilder.AppendLine();
            }

            _queryBuilder.Append(_indentation!.GetIndent());
        }     
    }
}
