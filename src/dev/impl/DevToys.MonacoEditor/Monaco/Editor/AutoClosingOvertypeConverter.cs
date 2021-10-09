#nullable enable

using Newtonsoft.Json;
using System;

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

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "always":
                    return AutoClosingOvertype.Always;
                case "auto":
                    return AutoClosingOvertype.Auto;
                case "never":
                    return AutoClosingOvertype.Never;
            }
            throw new Exception("Cannot unmarshal type AutoClosingOvertype");
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
