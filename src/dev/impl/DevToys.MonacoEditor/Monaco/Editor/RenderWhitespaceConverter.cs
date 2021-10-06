#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class RenderWhitespaceConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(RenderWhitespace) || t == typeof(RenderWhitespace?);
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
                    return RenderWhitespace.All;
                case "boundary":
                    return RenderWhitespace.Boundary;
                case "none":
                    return RenderWhitespace.None;
                case "selection":
                    return RenderWhitespace.Selection;
            }
            throw new Exception("Cannot unmarshal type RenderWhitespace");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (RenderWhitespace)untypedValue;
            switch (value)
            {
                case RenderWhitespace.All:
                    serializer.Serialize(writer, "all");
                    return;
                case RenderWhitespace.Boundary:
                    serializer.Serialize(writer, "boundary");
                    return;
                case RenderWhitespace.None:
                    serializer.Serialize(writer, "none");
                    return;
                case RenderWhitespace.Selection:
                    serializer.Serialize(writer, "selection");
                    return;
            }
            throw new Exception("Cannot marshal type RenderWhitespace");
        }
    }
}
