using System;
using System.Collections.Generic;
using System.Linq;


namespace DevToys.Models
{
    /// <summary>
    /// Wrapper for common characters and common operations on them.
    /// </summary>
    public sealed class SpecialCharacterDefinition : IEquatable<SpecialCharacterDefinition>
    {
        /// <summary>
        /// Keep a collection of our instances so we can iterate them if needed.
        /// </summary>
        private static readonly ICollection<SpecialCharacterDefinition> _definitions =
            new List<SpecialCharacterDefinition>();
        public static readonly SpecialCharacterDefinition LineFeed =
            new SpecialCharacterDefinition(SpecialCharacter.LineFeed, "\n", "\\n", @"LF (\n) (Linux)");
        public static readonly SpecialCharacterDefinition CarriageReturn =
            new SpecialCharacterDefinition(SpecialCharacter.CarriageReturn, "\r", "\\r", @"CR (\r) (Mac)");
        public static readonly SpecialCharacterDefinition CarriageReturnLinefeed =
            new SpecialCharacterDefinition(SpecialCharacter.CarriageReturnLineFeed, "\r\n", "\\r\\n", @"CRLF (\r\n) (Windows)");
        public static readonly SpecialCharacterDefinition Tab =
            new SpecialCharacterDefinition(SpecialCharacter.HorizontalTab, "\t", "\\t", "Tab");
        public static readonly SpecialCharacterDefinition Backspace =
            new SpecialCharacterDefinition(SpecialCharacter.Backspace, "\b", "\\b", "Backspace");
        public static readonly SpecialCharacterDefinition FormFeed =
            new SpecialCharacterDefinition(SpecialCharacter.FormFeed, "\f", "\\f", "Form Feed");
        public static readonly SpecialCharacterDefinition DoubleQuote =
            new SpecialCharacterDefinition(SpecialCharacter.DoubleQuote, "\"", "\\\"", "Double Quote");
        public static readonly SpecialCharacterDefinition Backslash =
            new SpecialCharacterDefinition(SpecialCharacter.Backslash, "\\", "\\\\", "Backslash");
        private SpecialCharacterDefinition(SpecialCharacter encoding, string value, string escaped, string displayName)
        {
            this.Type = encoding;
            this.Value = value;
            this.Escaped = escaped;
            this.DisplayName = displayName;
            SpecialCharacterDefinition._definitions.Add(this);
        }
        public SpecialCharacter Type { get; }
        public string Value { get; }
        public string Escaped { get; }
        public string DisplayName { get; }

        public static SpecialCharacterDefinition Parse(string value)
        {
            return SpecialCharacterDefinition._definitions.FirstOrDefault(x => x.Value == value);
        }

        public static SpecialCharacterDefinition Parse(SpecialCharacter value)
        {
            return SpecialCharacterDefinition._definitions.FirstOrDefault(x => x.Type == value);
        }
        public bool Equals(SpecialCharacterDefinition other) => other.Value == Value;

        public static implicit operator string(SpecialCharacterDefinition d) => d.Value;
    }
}


