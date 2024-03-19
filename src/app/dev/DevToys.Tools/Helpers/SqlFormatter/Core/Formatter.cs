using System.Text;
using System.Text.RegularExpressions;

namespace DevToys.Tools.Helpers.SqlFormatter.Core;

internal abstract class Formatter
{
    private static readonly Regex CommentWhitespacesRegex = new(@"\n[ \t]*", RegexOptions.Compiled, RegexFactory.DefaultMatchTimeout);

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
        return Format(query, new SqlFormatterOptions(Indentation: Models.Indentation.TwoSpaces, Uppercase: false));
    }

    /// <summary>
    /// Formats whitespace in a SQL string to make it easier to read.
    /// </summary>
    internal string Format(string query, SqlFormatterOptions options)
    {
        _options = options;
        _indentation = new Indentation(options.Indentation);
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
            return null;
        return _tokens![_index + n];
    }

    private void SetFormattedQueryFromTokens(ReadOnlySpan<char> querySpan)
    {
        _queryBuilder.Clear();

        Guard.IsNotNull(_tokens);
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
                    FormatTopLevelReservedWordNoIndent(token, querySpan);
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
        AppendToken(querySpan, token);
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
        Guard.IsNotNull(_indentation);
        return CommentWhitespacesRegex.Replace(comment.ToString(), $"\n{_indentation!.GetIndent()} ");
    }

    private void FormatTopLevelReservedWordNoIndent(Token token, ReadOnlySpan<char> querySpan)
    {
        Guard.IsNotNull(_indentation);
        _indentation!.DecreaseTopLevel();

        AddNewLine();

        AppendToken(querySpan, token);

        AddNewLine();
    }

    private void FormatTopLevelReservedWord(Token token, ReadOnlySpan<char> querySpan)
    {
        Guard.IsNotNull(_indentation);
        _indentation!.DecreaseTopLevel();

        AddNewLine();

        _indentation.IncreaseTopLevel();

        AppendToken(querySpan, token);

        AddNewLine();
    }

    private void FormatNewlineReservedWord(Token token, ReadOnlySpan<char> querySpan)
    {
        if (token.IsAnd(querySpan.Slice(token)))
        {
            Token? t = TokenLookBehind(2);

            if (t != null && t.Value.IsBetween(querySpan.Slice(t.Value)))
            {
                FormatWithSpaces(token, querySpan);
                return;
            }
        }
        AddNewLine();

        AppendToken(querySpan, token);

        _queryBuilder.Append(' ');
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

            if (behindToken is not { Type: TokenType.OpenParen or TokenType.LineComment or TokenType.Operator }
                && !behindToken?.IsValues(querySpan.Slice(behindToken!.Value)) == true)
            {
                _queryBuilder.TrimSpaceEnd();
            }
        }
        AppendToken(querySpan, token);

        Guard.IsNotNull(token);
        _inlineBlock.BeginIfPossible(_tokens!, _index, querySpan.Slice(token));

        if (!_inlineBlock.IsActive())
        {
            Guard.IsNotNull(_indentation);
            _indentation!.IncreaseBlockLevel();
            AddNewLine();
        }
    }

    /// <summary>
    /// Closing parentheses decrease the block indent level
    /// </summary>
    private void FormatClosingParentheses(Token token, ReadOnlySpan<char> querySpan)
    {
        Guard.IsNotNull(_indentation);
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
        Guard.IsNotNull(_params);

        string? value = null;
        ReadOnlySpan<char> valueSpan = querySpan.Slice(token);
        if (valueSpan.Length != 0)
            // assume the length of all placeholder is 1, since only char array is accepted 
            value = _params!.Get(valueSpan.Slice(0, 1).ToString());

        _queryBuilder.Append(value ?? querySpan.Slice(token).ToString());

        _queryBuilder.Append(' ');
    }

    /// <summary>
    /// Commas start a new line (unless within inline parentheses or SQL "LIMIT" clause)
    /// </summary>
    private void FormatComma(Token token, ReadOnlySpan<char> querySpan)
    {
        _queryBuilder.TrimSpaceEnd();

        if (_options.UseLeadingComma)
        {
            AddNewLine();
        }

        AppendToken(querySpan, token);

        _queryBuilder.Append(' ');

        if (_inlineBlock.IsActive())
        {
            return;
        }
        else if (_previousReservedToken is not null
            && _previousReservedToken.Value.IsLimit(querySpan.Slice(_previousReservedToken.Value)))
        {
            return;
        }

        if (!_options.UseLeadingComma)
        {
            AddNewLine();
        }
    }

    private void FormatWithSpaceAfter(Token token, ReadOnlySpan<char> querySpan)
    {
        _queryBuilder.TrimSpaceEnd();
        AppendToken(querySpan, token);
        _queryBuilder.Append(' ');
    }

    private void FormatWithoutSpaces(Token token, ReadOnlySpan<char> querySpan)
    {
        _queryBuilder.TrimSpaceEnd();
        AppendToken(querySpan, token);
    }

    private void FormatWithSpaces(Token token, ReadOnlySpan<char> querySpan)
    {
        AppendToken(querySpan, token);
        _queryBuilder.Append(' ');
    }

    private void FormatQuerySeparator(Token token, ReadOnlySpan<char> querySpan)
    {
        Guard.IsNotNull(_indentation);
        _indentation!.ResetIndentation();

        _queryBuilder.TrimSpaceEnd();
        AppendToken(querySpan, token);

        int times = _options.LinesBetweenQueries;

        while (times != 0)
        {
            _queryBuilder.AppendLine();
            times--;
        }
    }

    /// <summary>
    /// Appends the token value to the _queryBuilder, optionally uppercasing it.
    /// </summary>
    private void AppendToken(ReadOnlySpan<char> value, Token token)
    {
        if (_options.Uppercase
            && (token.Type is TokenType.Reserved
                or TokenType.ReservedTopLevel
                or TokenType.ReservedTopLevelNoIndent
                or TokenType.ReservedNewLine
                or TokenType.OpenParen
                or TokenType.CloseParen))
        {
            foreach (char c in value.Slice(token))
            {
                _queryBuilder.Append(char.ToUpper(c));
            }
        }
        else
        {
            _queryBuilder.Append(value.Slice(token));
        }
    }

    private void AddNewLine()
    {
        Guard.IsNotNull(_indentation);

        _queryBuilder.TrimSpaceEnd();

        if (_queryBuilder.Length != 0 && _queryBuilder[^1] != '\n')
            _queryBuilder.AppendLine();

        _queryBuilder.Append(_indentation!.GetIndent());
    }
}
