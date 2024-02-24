﻿using System.Text.RegularExpressions;

namespace DevToys.Tools.Helpers.SqlFormatter.Core;

internal static class RegexFactory
{
    internal static readonly TimeSpan DefaultMatchTimeout = TimeSpan.FromSeconds(1);
    private static readonly Regex SpecialCharacterRegex = new(@"[.*+?^${}()|[\]\\]", RegexOptions.Compiled, DefaultMatchTimeout);
    private static readonly Dictionary<string, string> Patterns = new()
    {
        { "``", "((`[^`]*($|`))+)" },
        { "{}", "((\\{[^\\}]*($|\\}))+)" },
        { "[]", "((\\[[^\\]]*($|\\]))(\\][^\\]]*($|\\]))*)" },
        { "\"\"", "((\"[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*(\"|$))+)" },
        { "''", "(('[^'\\\\]*(?:\\\\.[^'\\\\]*)*('|$))+)" },
        { "N''", "((N'[^'\\\\]*(?:\\\\.[^'\\\\]*)*('|$))+)" },
        { "U&''", "((U&'[^'\\\\]*(?:\\\\.[^'\\\\]*)*('|$))+)" },
        { "U&\"\"", "((U&\"[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*(\"|$))+)" },
        { "$$", "((?<tag>\\$\\w*\\$)[\\s\\S]*?(?:\\k<tag>|$))" }
    };

    internal static Regex CreateOperatorRegex(IEnumerable<string> multiLetterOperators)
    {
        IOrderedEnumerable<string> sortedOperators = SortByLengthDesc(multiLetterOperators);
        IEnumerable<string> escapedOperators = sortedOperators.Select(item => EscapeSpecialCharacters(item));
        string operators = string.Join("|", escapedOperators);
        return new Regex(@$"^({operators}|.)", RegexOptions.None, DefaultMatchTimeout);
    }

    internal static Regex CreateLineCommentRegex(string[] lineCommentTypes)
    {
        return new Regex($"^((?:{string.Join('|', lineCommentTypes.Select(item => EscapeSpecialCharacters(item)))}).*?)(?:\\r\\n|\\r|\\n|$)", RegexOptions.Singleline, DefaultMatchTimeout);
    }

    internal static Regex CreateReservedWordRegex(string[] reservedWords)
    {
        if (reservedWords.Length == 0)
            return new Regex(@"^\b$", RegexOptions.None, DefaultMatchTimeout);

        string reservedWordsPattern = string.Join('|', SortByLengthDesc(reservedWords)).Replace(" ", "\\s+");
        return new Regex(@$"^({reservedWordsPattern})\b", RegexOptions.IgnoreCase, DefaultMatchTimeout);
    }

    internal static Regex CreateWordRegex(string[] specialCharacters)
    {
        return new Regex(@"^([\p{L}\p{M}\p{Nd}\p{Pc}\p{Cf}\p{Cs}\p{Co}" + $"{string.Join(string.Empty, specialCharacters)}]+)", RegexOptions.None, DefaultMatchTimeout);
    }

    internal static Regex CreateStringRegex(string[] stringTypes)
    {
        return new Regex($"^({CreateStringPattern(stringTypes)})", RegexOptions.None, DefaultMatchTimeout);
    }

    /// <summary>
    /// This enables the following string patterns:
    /// 1. backtick quoted string using `` to escape
    /// 2. square bracket quoted string (SQL Server) using ]] to escape
    /// 3. double quoted string using "" or \" to escape
    /// 4. single quoted string using '' or \' to escape
    /// 5. national character quoted string using N'' or N\' to escape
    /// 6. Unicode single-quoted string using \' to escape
    /// 7. Unicode double-quoted string using \" to escape
    /// 8. PostgreSQL dollar-quoted strings
    /// </summary>
    internal static string CreateStringPattern(string[] stringTypes)
    {
        return string.Join('|', stringTypes.Select(item => Patterns[item]));
    }

    internal static Regex? CreatePlaceholderRegex(char[] types, string pattern)
    {
        if (types is null || types.Length == 0)
            return null;

        string typesRegex = string.Join('|', types.Select(item => EscapeSpecialCharacters(item.ToString())));

        return new Regex($"^((?:{typesRegex})(?:{pattern}))", RegexOptions.None, DefaultMatchTimeout);
    }

    internal static Regex CreateParenRegex(string[] parens)
    {
        return new Regex($"^({string.Join('|', parens.Select(item => EscapeParen(item)))})", RegexOptions.IgnoreCase, DefaultMatchTimeout);
    }

    private static string EscapeParen(string paren)
    {
        if (paren.Length == 1)
        {
            // A single punctuation character
            return EscapeSpecialCharacters(paren);
        }
        else
        {
            // longer word
            return $"\\b{paren}\\b";
        }
    }

    private static IOrderedEnumerable<string> SortByLengthDesc(IEnumerable<string> strings)
    {
        return strings.OrderByDescending(s => s.Length);
    }

    private static string EscapeSpecialCharacters(string input)
    {
        return SpecialCharacterRegex.Replace(input, "\\$&");
    }
}
