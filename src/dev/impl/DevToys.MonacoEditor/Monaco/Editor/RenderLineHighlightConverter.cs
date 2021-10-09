#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class RenderLineHighlightConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(RenderLineHighlight) || t == typeof(RenderLineHighlight?);
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
                case "all":
                    return RenderLineHighlight.All;
                case "gutter":
                    return RenderLineHighlight.Gutter;
                case "line":
                    return RenderLineHighlight.Line;
                case "none":
                    return RenderLineHighlight.None;
            }
            throw new Exception("Cannot unmarshal type RenderLineHighlight");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (RenderLineHighlight)untypedValue;
            switch (value)
            {
                case RenderLineHighlight.All:
                    serializer.Serialize(writer, "all");
                    return;
                case RenderLineHighlight.Gutter:
                    serializer.Serialize(writer, "gutter");
                    return;
                case RenderLineHighlight.Line:
                    serializer.Serialize(writer, "line");
                    return;
                case RenderLineHighlight.None:
                    serializer.Serialize(writer, "none");
                    return;
            }
            throw new Exception("Cannot marshal type RenderLineHighlight");
        }
    }
}
