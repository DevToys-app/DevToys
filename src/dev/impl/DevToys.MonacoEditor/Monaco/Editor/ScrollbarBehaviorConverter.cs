#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class ScrollbarBehaviorConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(ScrollbarBehavior) || t == typeof(ScrollbarBehavior?);
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
                "auto" => ScrollbarBehavior.Auto,
                "hidden" => ScrollbarBehavior.Hidden,
                "visible" => ScrollbarBehavior.Visible,
                _ => throw new Exception("Cannot unmarshal type ScrollbarBehavior"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (ScrollbarBehavior)untypedValue;
            switch (value)
            {
                case ScrollbarBehavior.Auto:
                    serializer.Serialize(writer, "auto");
                    return;
                case ScrollbarBehavior.Hidden:
                    serializer.Serialize(writer, "hidden");
                    return;
                case ScrollbarBehavior.Visible:
                    serializer.Serialize(writer, "visible");
                    return;
            }
            throw new Exception("Cannot marshal type ScrollbarBehavior");
        }
    }
}
