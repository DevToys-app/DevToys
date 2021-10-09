#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class MultipleConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(Multiple) || t == typeof(Multiple?);
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
                case "goto":
                    return Multiple.Goto;
                case "gotoAndPeek":
                    return Multiple.GotoAndPeek;
                case "peek":
                    return Multiple.Peek;
            }
            throw new Exception("Cannot unmarshal type Multiple");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Multiple)untypedValue;
            switch (value)
            {
                case Multiple.Goto:
                    serializer.Serialize(writer, "goto");
                    return;
                case Multiple.GotoAndPeek:
                    serializer.Serialize(writer, "gotoAndPeek");
                    return;
                case Multiple.Peek:
                    serializer.Serialize(writer, "peek");
                    return;
            }
            throw new Exception("Cannot marshal type Multiple");
        }
    }
}
