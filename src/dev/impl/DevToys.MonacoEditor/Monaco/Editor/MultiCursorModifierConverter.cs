#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class MultiCursorModifierConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(MultiCursorModifier) || t == typeof(MultiCursorModifier?);
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
                "alt" => MultiCursorModifier.Alt,
                "ctrlCmd" => MultiCursorModifier.CtrlCmd,
                _ => throw new Exception("Cannot unmarshal type MultiCursorModifier"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (MultiCursorModifier)untypedValue;
            switch (value)
            {
                case MultiCursorModifier.Alt:
                    serializer.Serialize(writer, "alt");
                    return;
                case MultiCursorModifier.CtrlCmd:
                    serializer.Serialize(writer, "ctrlCmd");
                    return;
            }

            throw new Exception("Cannot marshal type MultiCursorModifier");
        }
    }
}
