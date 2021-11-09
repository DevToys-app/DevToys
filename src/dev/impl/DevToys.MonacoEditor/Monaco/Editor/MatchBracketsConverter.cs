#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class MatchBracketsConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(MatchBrackets) || t == typeof(MatchBrackets?);

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            string? value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "always" => MatchBrackets.Always,
                "near" => MatchBrackets.Near,
                "never" => MatchBrackets.Never,
                _ => throw new Exception("Cannot unmarshal type MatchBrackets"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (MatchBrackets)untypedValue;
            switch (value)
            {
                case MatchBrackets.Always:
                    serializer.Serialize(writer, "always");
                    return;
                case MatchBrackets.Near:
                    serializer.Serialize(writer, "near");
                    return;
                case MatchBrackets.Never:
                    serializer.Serialize(writer, "never");
                    return;
            }
            throw new Exception("Cannot marshal type MatchBrackets");
        }
    }
}
