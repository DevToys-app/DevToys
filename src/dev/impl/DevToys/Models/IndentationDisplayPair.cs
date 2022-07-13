#nullable enable

using System;

namespace DevToys.Models
{
    public class IndentationDisplayPair : IEquatable<IndentationDisplayPair>
    {
        private static JsonFormatterStrings Strings => LanguageManager.Instance.JsonFormatter;

        public static readonly IndentationDisplayPair TwoSpaces = new(Strings.TwoSpaces, Indentation.TwoSpaces);

        public static readonly IndentationDisplayPair FourSpaces = new(Strings.FourSpaces, Indentation.FourSpaces);

        public static readonly IndentationDisplayPair OneTab = new(Strings.OneTab, Indentation.OneTab);

        public static readonly IndentationDisplayPair Minified = new(Strings.Minified, Indentation.Minified);

        public string DisplayName { get; }

        public Indentation Value { get; }

        private IndentationDisplayPair(string displayName, Indentation value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(IndentationDisplayPair other)
        {
            return other.Value == Value;
        }
    }
}
