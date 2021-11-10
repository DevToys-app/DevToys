#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class LineNumbersTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(LineNumbersType) || t == typeof(LineNumbersType?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            string? value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "interval" => LineNumbersType.Interval,
                "off" => LineNumbersType.Off,
                "on" => LineNumbersType.On,
                "relative" => LineNumbersType.Relative,
                _ => throw new Exception("Cannot unmarshal type LineNumbersType"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (LineNumbersType)untypedValue;
            switch (value)
            {
                case LineNumbersType.Interval:
                    serializer.Serialize(writer, "interval");
                    return;
                case LineNumbersType.Off:
                    serializer.Serialize(writer, "off");
                    return;
                case LineNumbersType.On:
                    serializer.Serialize(writer, "on");
                    return;
                case LineNumbersType.Relative:
                    serializer.Serialize(writer, "relative");
                    return;
            }
            throw new Exception("Cannot marshal type LineNumbersType");
        }
    }
}
