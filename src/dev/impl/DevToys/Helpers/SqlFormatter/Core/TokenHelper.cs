#nullable enable

using System;
using System.Text.RegularExpressions;

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal static class TokenHelper
    {
        private static readonly TimeSpan TimeOut = TimeSpan.FromMilliseconds(500);
        private static readonly Regex AndRegex = new("^AND$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);
        private static readonly Regex BetweenRegex = new("^BETWEEN$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);
        private static readonly Regex LimitRegex = new("^LIMIT$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);
        private static readonly Regex SetRegex = new("^SET$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);
        private static readonly Regex ByRegex = new("^BY$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);
        private static readonly Regex WindowRegex = new("^WINDOW$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);
        private static readonly Regex EndRegex = new("^END$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeOut);

        internal static bool IsAnd(Token? token)
        {
            return IsToken(token, TokenType.ReservedNewLine, AndRegex);
        }

        internal static bool isBetween(Token? token)
        {
            return IsToken(token, TokenType.Reserved, BetweenRegex);
        }

        internal static bool isLimit(Token? token)
        {
            return IsToken(token, TokenType.ReservedTopLevel, LimitRegex);
        }

        internal static bool isSet(Token? token)
        {
            return IsToken(token, TokenType.ReservedTopLevel, SetRegex);
        }

        internal static bool isBy(Token? token)
        {
            return IsToken(token, TokenType.Reserved, ByRegex);
        }

        internal static bool isWindow(Token? token)
        {
            return IsToken(token, TokenType.ReservedTopLevel, WindowRegex);
        }

        internal static bool isEnd(Token? token)
        {
            return IsToken(token, TokenType.CloseParen, EndRegex);
        }

        private static bool IsToken(Token? token, TokenType type, Regex regex)
        {
            return token?.Type == type && regex.IsMatch(token?.Value);
        }
    }
}
