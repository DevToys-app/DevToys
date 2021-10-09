#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class WordWrapConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(WordWrap) || t == typeof(WordWrap?);
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
                case "bounded":
                    return WordWrap.Bounded;
                case "off":
                    return WordWrap.Off;
                case "on":
                    return WordWrap.On;
                case "wordWrapColumn":
                    return WordWrap.WordWrapColumn;
            }
            throw new Exception("Cannot unmarshal type WordWrap");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (WordWrap)untypedValue;
            switch (value)
            {
                case WordWrap.Bounded:
                    serializer.Serialize(writer, "bounded");
                    return;
                case WordWrap.Off:
                    serializer.Serialize(writer, "off");
                    return;
                case WordWrap.On:
                    serializer.Serialize(writer, "on");
                    return;
                case WordWrap.WordWrapColumn:
                    serializer.Serialize(writer, "wordWrapColumn");
                    return;
            }
            throw new Exception("Cannot marshal type WordWrap");
        }
    }
}
