#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class AutoClosingOvertypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(AutoClosingOvertype) || t == typeof(AutoClosingOvertype?);
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
                "always" => AutoClosingOvertype.Always,
                "auto" => AutoClosingOvertype.Auto,
                "never" => AutoClosingOvertype.Never,
                _ => throw new Exception("Cannot unmarshal type AutoClosingOvertype"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (AutoClosingOvertype)untypedValue;
            switch (value)
            {
                case AutoClosingOvertype.Always:
                    serializer.Serialize(writer, "always");
                    return;
                case AutoClosingOvertype.Auto:
                    serializer.Serialize(writer, "auto");
                    return;
                case AutoClosingOvertype.Never:
                    serializer.Serialize(writer, "never");
                    return;
            }
            throw new Exception("Cannot marshal type AutoClosingOvertype");
        }
    }
}
