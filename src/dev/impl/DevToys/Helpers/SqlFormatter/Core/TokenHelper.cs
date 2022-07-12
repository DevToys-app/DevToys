#nullable enable

using System;
using System.Text.RegularExpressions;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal static class TokenHelper
    {
        internal static bool IsAnd(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedNewLine, tokenValueSpan, "AND".AsSpan());
        }

        internal static bool IsBetween(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.Reserved, tokenValueSpan, "BETWEEN".AsSpan());
        }

        internal static bool IsLimit(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedTopLevel, tokenValueSpan, "LIMIT".AsSpan());
        }

        internal static bool IsSet(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedTopLevel, tokenValueSpan, "SET".AsSpan());
        }

        internal static bool IsBy(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.Reserved, tokenValueSpan, "BY".AsSpan());
        }

        internal static bool IsWindow(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.ReservedTopLevel, tokenValueSpan, "WINDOW".AsSpan());
        }

        internal static bool IsEnd(this Token token, ReadOnlySpan<char> tokenValueSpan)
        {
            return IsToken(token.Type, TokenType.CloseParen, tokenValueSpan, "END".AsSpan());
        }

        private static bool IsToken(TokenType type, TokenType otherType,
            ReadOnlySpan<char> tokenValueSpan, ReadOnlySpan<char> otherSpan)
        {
            return type == otherType &&
                   tokenValueSpan.Equals(otherSpan, StringComparison.OrdinalIgnoreCase);
        }
    }
}
