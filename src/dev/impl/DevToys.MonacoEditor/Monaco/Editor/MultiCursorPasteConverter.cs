#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class MultiCursorPasteConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(MultiCursorPaste) || t == typeof(MultiCursorPaste?);
        }

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            string? value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "full" => MultiCursorPaste.Full,
                "spread" => MultiCursorPaste.Spread,
                _ => throw new Exception("Cannot unmarshal type MultiCursorPaste"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (MultiCursorPaste)untypedValue;
            switch (value)
            {
                case MultiCursorPaste.Full:
                    serializer.Serialize(writer, "full");
                    return;
                case MultiCursorPaste.Spread:
                    serializer.Serialize(writer, "spread");
                    return;
            }

            throw new Exception("Cannot marshal type MultiCursorPaste");
        }
    }
}
