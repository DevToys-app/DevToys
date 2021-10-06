#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class AutoClosingQuotesConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(AutoClosingQuotes) || t == typeof(AutoClosingQuotes?);
        }

        public override object ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "always":
                    return AutoClosingQuotes.Always;
                case "beforeWhitespace":
                    return AutoClosingQuotes.BeforeWhitespace;
                case "languageDefined":
                    return AutoClosingQuotes.LanguageDefined;
                case "never":
                    return AutoClosingQuotes.Never;
            }

            throw new Exception("Cannot unmarshal type AutoClosingQuotes");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (AutoClosingQuotes)untypedValue;
            switch (value)
            {
                case AutoClosingQuotes.Always:
                    serializer.Serialize(writer, "always");
                    return;
                case AutoClosingQuotes.BeforeWhitespace:
                    serializer.Serialize(writer, "beforeWhitespace");
                    return;
                case AutoClosingQuotes.LanguageDefined:
                    serializer.Serialize(writer, "languageDefined");
                    return;
                case AutoClosingQuotes.Never:
                    serializer.Serialize(writer, "never");
                    return;
            }

            throw new Exception("Cannot marshal type AutoClosingQuotes");
        }

        public static readonly AutoClosingQuotesConverter Singleton = new AutoClosingQuotesConverter();
    }
}
