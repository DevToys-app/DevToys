#nullable enable

using Newtonsoft.Json;
using System;

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

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "full":
                    return MultiCursorPaste.Full;
                case "spread":
                    return MultiCursorPaste.Spread;
            }

            throw new Exception("Cannot unmarshal type MultiCursorPaste");
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
