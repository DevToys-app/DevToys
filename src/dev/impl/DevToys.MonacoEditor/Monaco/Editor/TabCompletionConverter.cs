#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class TabCompletionConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(TabCompletion) || t == typeof(TabCompletion?);
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
                "off" => TabCompletion.Off,
                "on" => TabCompletion.On,
                "onlySnippets" => TabCompletion.OnlySnippets,
                _ => throw new Exception("Cannot unmarshal type TabCompletion"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TabCompletion)untypedValue;
            switch (value)
            {
                case TabCompletion.Off:
                    serializer.Serialize(writer, "off");
                    return;
                case TabCompletion.On:
                    serializer.Serialize(writer, "on");
                    return;
                case TabCompletion.OnlySnippets:
                    serializer.Serialize(writer, "onlySnippets");
                    return;
            }
            throw new Exception("Cannot marshal type TabCompletion");
        }
    }
}
