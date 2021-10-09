#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class FoldingStrategyConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(FoldingStrategy) || t == typeof(FoldingStrategy?);
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
                case "auto":
                    return FoldingStrategy.Auto;
                case "indentation":
                    return FoldingStrategy.Indentation;
            }

            throw new Exception("Cannot unmarshal type FoldingStrategy");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (FoldingStrategy)untypedValue;
            switch (value)
            {
                case FoldingStrategy.Auto:
                    serializer.Serialize(writer, "auto");
                    return;
                case FoldingStrategy.Indentation:
                    serializer.Serialize(writer, "indentation");
                    return;
            }

            throw new Exception("Cannot marshal type FoldingStrategy");
        }
    }
}
