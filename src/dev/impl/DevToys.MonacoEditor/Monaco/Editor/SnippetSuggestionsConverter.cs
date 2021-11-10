#nullable enable

using System;
using Newtonsoft.Json;

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

            string? value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "bottom" => SnippetSuggestions.Bottom,
                "inline" => SnippetSuggestions.Inline,
                "none" => SnippetSuggestions.None,
                "top" => SnippetSuggestions.Top,
                _ => throw new Exception("Cannot unmarshal type SnippetSuggestions"),
            };
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
