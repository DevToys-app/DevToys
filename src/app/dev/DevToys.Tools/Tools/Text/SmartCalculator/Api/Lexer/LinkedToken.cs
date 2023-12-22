namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

/// <summary>
/// Represents a token and expose the access to the previous and next token.
/// </summary>
[DebuggerDisplay($"Token = {{{nameof(Token)}.{nameof(IToken.GetText)}()}}")]
public sealed class LinkedToken
{
    private readonly Lazy<LinkedToken?> _nextToken;

    /// <summary>
    /// Gets the token before the current one.
    /// </summary>
    public LinkedToken? Previous { get; }

    /// <summary>
    /// Gets the token after the current one.
    /// </summary>
    public LinkedToken? Next => _nextToken.Value;

    /// <summary>
    /// Gets the current token information.
    /// </summary>
    public IToken Token { get; }

    internal LinkedToken(LinkedToken? previous, IToken token, ITokenEnumerator tokenEnumerator)
    {
        Guard.IsNotNull(token);
        Guard.IsNotNull(tokenEnumerator);

        Previous = previous;
        Token = token;

        _nextToken
            = new Lazy<LinkedToken?>(() =>
            {
                if (tokenEnumerator.MoveNext())
                {
                    Guard.IsNotNull(tokenEnumerator.Current);
                    return new LinkedToken(
                            previous: this,
                            token: tokenEnumerator.Current,
                            tokenEnumerator);
                }
                else if (tokenEnumerator.Current is not null)
                {
                    return new LinkedToken(
                            previous: this,
                            token: tokenEnumerator.Current);
                }

                return null;
            });
    }

    private LinkedToken(LinkedToken? previous, IToken token)
    {
        Guard.IsNotNull(token);
        Previous = previous;
        Token = token;
        _nextToken = new Lazy<LinkedToken?>(() => null);
    }

    public override string? ToString()
    {
        return Token.ToString();
    }
}
