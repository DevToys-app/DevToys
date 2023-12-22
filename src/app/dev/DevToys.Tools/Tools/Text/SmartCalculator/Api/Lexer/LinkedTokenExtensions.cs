namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

public static class LinkedTokenExtensions
{
    /// <summary>
    /// Gets the next token to the right of <paramref name="tokenToJump"/>.
    /// </summary>
    public static LinkedToken? GetTokenAfter(this LinkedToken? sourceToken, LinkedToken tokenToJump)
    {
        while (sourceToken is not null && sourceToken.Token.EndInLine < tokenToJump.Token.EndInLine)
        {
            sourceToken = sourceToken.Next;
        }

        return sourceToken;
    }

    /// <summary>
    /// Jumps to the last token.
    /// </summary>
    public static LinkedToken? SkipToLastToken(this LinkedToken? currentToken)
    {
        while (currentToken?.Next is not null)
        {
            currentToken = currentToken.Next;
        }

        return currentToken;
    }

    /// <summary>
    /// Assuming that the <paramref name="currentToken"/> is a <see cref="PredefinedTokenAndDataTypeNames.Word"/>,
    /// jumps to the next token that isn't a <see cref="PredefinedTokenAndDataTypeNames.Word"/>.
    /// </summary>
    public static LinkedToken? SkipNextWordTokens(this LinkedToken? currentToken)
    {
        while (currentToken is not null && currentToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.Word))
        {
            currentToken = currentToken.Next;
        }

        return currentToken;
    }

    /// <summary>
    /// Assuming that the <paramref name="currentToken"/> has the <paramref name="expectedTokenType"/>,
    /// skip to the next token that doesn't has the <paramref name="expectedTokenType"/> and return it.
    /// Optionally, if the <paramref name="currentToken"/> is a <see cref="PredefinedTokenAndDataTypeNames.Word"/>,
    /// skip it along with all the following <see cref="PredefinedTokenAndDataTypeNames.Word"/>.
    /// </summary>
    public static bool SkipToken(
        this LinkedToken? currentToken,
        string expectedTokenType,
        bool skipWordsToken,
        out LinkedToken? nextToken)
    {
        LinkedToken? backupToken = currentToken;
        if (skipWordsToken)
            currentToken = currentToken.SkipNextWordTokens();

        if (currentToken is null || currentToken.Token.IsNotOfType(expectedTokenType))
        {
            nextToken = backupToken;
            return false;
        }

        nextToken = currentToken.Next;
        return true;
    }

    /// <summary>
    /// Jumps to the next token that has the given <paramref name="tokenType"/>.
    /// </summary>
    public static bool JumpToNextTokenOfType(
        this LinkedToken? currentToken,
        string tokenType,
        out LinkedToken? nextToken)
    {
        return currentToken.JumpToNextTokenOfType(tokenType, string.Empty, out nextToken);
    }

    /// <summary>
    /// Jumps to the next token that has the given <paramref name="tokenType"/> and <paramref name="tokenText"/>.
    /// </summary>
    public static bool JumpToNextTokenOfType(
        this LinkedToken? currentToken,
        string tokenType,
        string tokenText,
        out LinkedToken? nextToken)
    {
        while (currentToken is not null)
        {
            if (string.IsNullOrEmpty(tokenText))
            {
                if (currentToken.Token.IsOfType(tokenType))
                {
                    nextToken = currentToken;
                    return true;
                }
            }
            else
            {
                if (currentToken.Token.Is(tokenType, tokenText))
                {
                    nextToken = currentToken;
                    return true;
                }
            }

            currentToken = currentToken.Next;
        }

        nextToken = null;
        return false;
    }
}
