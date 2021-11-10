#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    internal class TextDecorationConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TextDecoration) || t == typeof(TextDecoration?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            string? value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "none" => TextDecoration.None,
                "underline" => TextDecoration.Underline,
                "overline" => TextDecoration.Overline,
                "line-through" => TextDecoration.LineThrough,
                "initial" => TextDecoration.Initial,
                "inherit" => TextDecoration.Inherit,
                _ => throw new Exception("Cannot unmarshal type TextDecoration"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TextDecoration)untypedValue;
            switch (value)
            {
                case TextDecoration.None:
                    serializer.Serialize(writer, "none");
                    return;
                case TextDecoration.Underline:
                    serializer.Serialize(writer, "underline");
                    return;
                case TextDecoration.Overline:
                    serializer.Serialize(writer, "overline");
                    return;
                case TextDecoration.LineThrough:
                    serializer.Serialize(writer, "line-through");
                    return;
                case TextDecoration.Initial:
                    serializer.Serialize(writer, "initial");
                    return;
                case TextDecoration.Inherit:
                    serializer.Serialize(writer, "inherit");
                    return;
            }
            throw new Exception("Cannot marshal type TextDecoration");
        }
    }
}
