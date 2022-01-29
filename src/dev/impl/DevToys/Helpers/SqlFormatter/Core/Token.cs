#nullable enable

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal sealed class Token
    {
        internal string Value { get; }

        internal TokenType Type { get; }

        internal string? WhitespaceBefore { get; set; }

        internal string? Key { get; set; }

        public Token(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }
    }
}
