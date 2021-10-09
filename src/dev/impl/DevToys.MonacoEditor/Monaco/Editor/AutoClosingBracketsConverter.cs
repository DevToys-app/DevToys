#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class AutoClosingBracketsConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(AutoClosingBrackets) || t == typeof(AutoClosingBrackets?);
        }

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "always":
                    return AutoClosingBrackets.Always;
                case "beforeWhitespace":
                    return AutoClosingBrackets.BeforeWhitespace;
                case "languageDefined":
                    return AutoClosingBrackets.LanguageDefined;
                case "never":
                    return AutoClosingBrackets.Never;
            }

            throw new Exception("Cannot unmarshal type AutoClosingBrackets");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (AutoClosingBrackets)untypedValue;
            switch (value)
            {
                case AutoClosingBrackets.Always:
                    serializer.Serialize(writer, "always");
                    return;
                case AutoClosingBrackets.BeforeWhitespace:
                    serializer.Serialize(writer, "beforeWhitespace");
                    return;
                case AutoClosingBrackets.LanguageDefined:
                    serializer.Serialize(writer, "languageDefined");
                    return;
                case AutoClosingBrackets.Never:
                    serializer.Serialize(writer, "never");
                    return;
            }

            throw new Exception("Cannot marshal type AutoClosingBrackets");
        }
    }
}
