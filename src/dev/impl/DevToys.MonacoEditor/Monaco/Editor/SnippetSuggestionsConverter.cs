#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class SnippetSuggestionsConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(SnippetSuggestions) || t == typeof(SnippetSuggestions?);
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
                case "bottom":
                    return SnippetSuggestions.Bottom;
                case "inline":
                    return SnippetSuggestions.Inline;
                case "none":
                    return SnippetSuggestions.None;
                case "top":
                    return SnippetSuggestions.Top;
            }
            throw new Exception("Cannot unmarshal type SnippetSuggestions");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SnippetSuggestions)untypedValue;
            switch (value)
            {
                case SnippetSuggestions.Bottom:
                    serializer.Serialize(writer, "bottom");
                    return;
                case SnippetSuggestions.Inline:
                    serializer.Serialize(writer, "inline");
                    return;
                case SnippetSuggestions.None:
                    serializer.Serialize(writer, "none");
                    return;
                case SnippetSuggestions.Top:
                    serializer.Serialize(writer, "top");
                    return;
            }
            throw new Exception("Cannot marshal type SnippetSuggestions");
        }
    }
}
