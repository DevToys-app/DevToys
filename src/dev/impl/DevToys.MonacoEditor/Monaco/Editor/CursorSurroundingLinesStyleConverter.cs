#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class CursorSurroundingLinesStyleConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(CursorSurroundingLinesStyle) || t == typeof(CursorSurroundingLinesStyle?);
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
                "all" => CursorSurroundingLinesStyle.All,
                "default" => CursorSurroundingLinesStyle.Default,
                _ => throw new Exception("Cannot unmarshal type CursorSurroundingLinesStyle"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (CursorSurroundingLinesStyle)untypedValue;
            switch (value)
            {
                case CursorSurroundingLinesStyle.All:
                    serializer.Serialize(writer, "all");
                    return;
                case CursorSurroundingLinesStyle.Default:
                    serializer.Serialize(writer, "default");
                    return;
            }
            throw new Exception("Cannot marshal type CursorSurroundingLinesStyle");
        }
    }
}
