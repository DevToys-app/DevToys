#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class CursorStyleConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(CursorStyle) || t == typeof(CursorStyle?);
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
                "block" => CursorStyle.Block,
                "block-outline" => CursorStyle.BlockOutline,
                "line" => CursorStyle.Line,
                "line-thin" => CursorStyle.LineThin,
                "underline" => CursorStyle.Underline,
                "underline-thin" => CursorStyle.UnderlineThin,
                _ => throw new Exception("Cannot unmarshal type CursorStyle"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (CursorStyle)untypedValue;
            switch (value)
            {
                case CursorStyle.Block:
                    serializer.Serialize(writer, "block");
                    return;
                case CursorStyle.BlockOutline:
                    serializer.Serialize(writer, "block-outline");
                    return;
                case CursorStyle.Line:
                    serializer.Serialize(writer, "line");
                    return;
                case CursorStyle.LineThin:
                    serializer.Serialize(writer, "line-thin");
                    return;
                case CursorStyle.Underline:
                    serializer.Serialize(writer, "underline");
                    return;
                case CursorStyle.UnderlineThin:
                    serializer.Serialize(writer, "underline-thin");
                    return;
            }

            throw new Exception("Cannot marshal type CursorStyle");
        }
    }
}
