#nullable enable

using Newtonsoft.Json;
using System;

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

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "off":
                    return TabCompletion.Off;
                case "on":
                    return TabCompletion.On;
                case "onlySnippets":
                    return TabCompletion.OnlySnippets;
            }
            throw new Exception("Cannot unmarshal type TabCompletion");
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
