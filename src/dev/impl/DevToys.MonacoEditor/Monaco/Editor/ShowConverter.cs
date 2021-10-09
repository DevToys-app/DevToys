#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class ShowConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(Show) || t == typeof(Show?);
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
                    return Show.Always;
                case "mouseover":
                    return Show.Mouseover;
            }
            throw new Exception("Cannot unmarshal type Show");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Show)untypedValue;
            switch (value)
            {
                case Show.Always:
                    serializer.Serialize(writer, "always");
                    return;
                case Show.Mouseover:
                    serializer.Serialize(writer, "mouseover");
                    return;
            }
            throw new Exception("Cannot marshal type Show");
        }
    }
}
