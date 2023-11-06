using System;

namespace DevToys.Models
{
    public sealed class NewlineEncodingDisplayPair : IEquatable<NewlineEncodingDisplayPair>
    {
        public static readonly NewlineEncodingDisplayPair Linefeed = new(@"LF (\n) (Linux)", SpecialCharacter.LineFeed);
        public static readonly NewlineEncodingDisplayPair CarriageReturn = new(@"CR (\r) (Mac)", SpecialCharacter.CarriageReturn);
        public static readonly NewlineEncodingDisplayPair CarriageReturnLineFeed = new(@"CRLF (\r\n) (Windows)", SpecialCharacter.CarriageReturnLineFeed);

        public string DisplayName { get; }

        public SpecialCharacter Value { get; }

        private NewlineEncodingDisplayPair(string displayName, SpecialCharacter value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(NewlineEncodingDisplayPair other) => other.Value == Value;
    }
}


