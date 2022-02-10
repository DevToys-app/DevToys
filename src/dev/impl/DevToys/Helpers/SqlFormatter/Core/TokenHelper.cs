#nullable enable

using System;
using System.Text.RegularExpressions;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal static class TokenHelper
    {
        public static bool IsAnd(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedNewLine, tokenValueSpan, "AND".AsSpan());
        }

        public static bool isBetween(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.Reserved, tokenValueSpan, "BETWEEN".AsSpan());
        }

        public static bool isLimit(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedTopLevel, tokenValueSpan, "LIMIT".AsSpan());
        }

        public static bool isSet(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedTopLevel, tokenValueSpan, "SET".AsSpan());
        }

        public static bool isBy(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.Reserved, tokenValueSpan, "BY".AsSpan());
        }

        public static bool isWindow(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedTopLevel, tokenValueSpan, "WINDOW".AsSpan());
        }

        public static bool isEnd(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.CloseParen, tokenValueSpan, "END".AsSpan());
        }

        public static bool IsToken(TokenType type, TokenType otherType,
            ReadOnlySpan<char> tokenValueSpan, ReadOnlySpan<char> otherSpan)
        {
            return type == otherType &&
                   tokenValueSpan.Equals(otherSpan, StringComparison.OrdinalIgnoreCase);
        }
    }
}
