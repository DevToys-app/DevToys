using System;

namespace DevToys.Models
{
    public enum Indentations
    {
        TwoSpaces,
        FourSpaces,
        OneTab,
        Minified
    }

    public class Indentation : IEquatable<Indentation>
    {
        private static JsonFormatterStrings Strings => LanguageManager.Instance.JsonFormatter;

        public static readonly Indentation TwoSpaces = new Indentation(Strings.TwoSpaces, Indentations.TwoSpaces);

        public static readonly Indentation FourSpaces = new Indentation(Strings.FourSpaces, Indentations.FourSpaces);

        public static readonly Indentation OneTab = new Indentation(Strings.OneTab, Indentations.OneTab);

        public static readonly Indentation Minified = new Indentation(Strings.Minified, Indentations.Minified);

        public string DisplayName { get; }

        public Indentations Value { get; }

        public Indentation(string displayName, Indentations value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(Indentation other)
        {
            return other.Value == Value;
        }
    }
}
