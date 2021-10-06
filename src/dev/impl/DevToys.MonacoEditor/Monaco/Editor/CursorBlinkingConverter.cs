#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class CursorBlinkingConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CursorBlinking) || t == typeof(CursorBlinking?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "blink":
                    return CursorBlinking.Blink;
                case "expand":
                    return CursorBlinking.Expand;
                case "phase":
                    return CursorBlinking.Phase;
                case "smooth":
                    return CursorBlinking.Smooth;
                case "solid":
                    return CursorBlinking.Solid;
            }

            throw new Exception("Cannot unmarshal type CursorBlinking");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (CursorBlinking)untypedValue;
            switch (value)
            {
                case CursorBlinking.Blink:
                    serializer.Serialize(writer, "blink");
                    return;
                case CursorBlinking.Expand:
                    serializer.Serialize(writer, "expand");
                    return;
                case CursorBlinking.Phase:
                    serializer.Serialize(writer, "phase");
                    return;
                case CursorBlinking.Smooth:
                    serializer.Serialize(writer, "smooth");
                    return;
                case CursorBlinking.Solid:
                    serializer.Serialize(writer, "solid");
                    return;
            }
            throw new Exception("Cannot marshal type CursorBlinking");
        }
    }
}
