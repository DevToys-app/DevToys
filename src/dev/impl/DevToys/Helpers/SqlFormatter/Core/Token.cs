#nullable enable

namespace DevToys.Helpers.SqlFormatter.Core
{
    internal struct Token
    {
        internal readonly int Index { get; }
        internal readonly int Length { get; }
        internal int PrecedingWitespaceLength { get; set; }
        internal readonly TokenType Type { get; }

        public Token(int index, int length, TokenType type, int precedingWitespaceLength = 0)
        {
            Index = index;
            Length = length;
            Type = type;
            PrecedingWitespaceLength = precedingWitespaceLength;
        }
    }
}
