using System;
using System.Collections.Generic;

namespace DevToys.Models
{
    public class NewlineSeparatorDisplayPair : IEquatable<NewlineSeparatorDisplayPair>
    {
        private static Base64EncoderDecoderStrings Strings => LanguageManager.Instance.Base64EncoderDecoder;

        public static readonly NewlineSeparatorDisplayPair CRLF = new (Strings.CarriageReturnLineFeed, NewlineSeparator.CRLF, "\r\n");

        public static readonly NewlineSeparatorDisplayPair LF = new (Strings.LineFeed, NewlineSeparator.LF, "\n");

        public static readonly IReadOnlyList<NewlineSeparatorDisplayPair> Values = new List<NewlineSeparatorDisplayPair> { CRLF, LF };

        public string DisplayName { get; }
        public NewlineSeparator Value { get; }
        public string EscapeSequence { get; }

        private NewlineSeparatorDisplayPair(string displayName, NewlineSeparator value, string escapeSequence)
        {
            DisplayName = displayName;
            Value = value;
            EscapeSequence = escapeSequence;
        }

        public bool Equals(NewlineSeparatorDisplayPair other)
        {
            return other.Value == Value;
        }
    }
}
