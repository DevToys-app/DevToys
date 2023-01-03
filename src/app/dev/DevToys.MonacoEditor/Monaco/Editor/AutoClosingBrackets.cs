using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Options for auto closing brackets.
/// Defaults to language defined behavior.
/// </summary>
[JsonConverter(typeof(AutoClosingBracketsConverter))]
public enum AutoClosingBrackets { Always, BeforeWhitespace, LanguageDefined, Never };

internal class AutoClosingBracketsConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(AutoClosingBrackets) || t == typeof(AutoClosingBrackets?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "always" => AutoClosingBrackets.Always,
            "beforeWhitespace" => AutoClosingBrackets.BeforeWhitespace,
            "languageDefined" => AutoClosingBrackets.LanguageDefined,
            "never" => (object)AutoClosingBrackets.Never,
            _ => throw new Exception("Cannot unmarshal type AutoClosingBrackets"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (AutoClosingBrackets)untypedValue;
        switch (value)
        {
            case AutoClosingBrackets.Always:
                serializer.Serialize(writer, "always");
                return;
            case AutoClosingBrackets.BeforeWhitespace:
                serializer.Serialize(writer, "beforeWhitespace");
                return;
            case AutoClosingBrackets.LanguageDefined:
                serializer.Serialize(writer, "languageDefined");
                return;
            case AutoClosingBrackets.Never:
                serializer.Serialize(writer, "never");
                return;
        }
        throw new Exception("Cannot marshal type AutoClosingBrackets");
    }
}
