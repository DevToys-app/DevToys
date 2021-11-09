#nullable enable

using System;
using Newtonsoft.Json;

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

            string? value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "all" => RenderWhitespace.All,
                "boundary" => RenderWhitespace.Boundary,
                "none" => RenderWhitespace.None,
                "selection" => RenderWhitespace.Selection,
                _ => throw new Exception("Cannot unmarshal type RenderWhitespace"),
            };
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
