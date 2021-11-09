#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class WrappingIndentConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(WrappingIndent) || t == typeof(WrappingIndent?);
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
                "deepIndent" => WrappingIndent.DeepIndent,
                "indent" => WrappingIndent.Indent,
                "none" => WrappingIndent.None,
                "same" => WrappingIndent.Same,
                _ => throw new Exception("Cannot unmarshal type WrappingIndent"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (WrappingIndent)untypedValue;
            switch (value)
            {
                case WrappingIndent.DeepIndent:
                    serializer.Serialize(writer, "deepIndent");
                    return;
                case WrappingIndent.Indent:
                    serializer.Serialize(writer, "indent");
                    return;
                case WrappingIndent.None:
                    serializer.Serialize(writer, "none");
                    return;
                case WrappingIndent.Same:
                    serializer.Serialize(writer, "same");
                    return;
            }
            throw new Exception("Cannot marshal type WrappingIndent");
        }
    }
}
