using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Options for auto surrounding.
/// Defaults to always allowing auto surrounding.
/// </summary>
[JsonConverter(typeof(AutoSurroundConverter))]
public enum AutoSurround { Brackets, LanguageDefined, Never, Quotes };

internal class AutoSurroundConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(AutoSurround) || t == typeof(AutoSurround?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "brackets" => AutoSurround.Brackets,
            "languageDefined" => AutoSurround.LanguageDefined,
            "never" => AutoSurround.Never,
            "quotes" => (object)AutoSurround.Quotes,
            _ => throw new Exception("Cannot unmarshal type AutoSurround"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (AutoSurround)untypedValue;
        switch (value)
        {
            case AutoSurround.Brackets:
                serializer.Serialize(writer, "brackets");
                return;
            case AutoSurround.LanguageDefined:
                serializer.Serialize(writer, "languageDefined");
                return;
            case AutoSurround.Never:
                serializer.Serialize(writer, "never");
                return;
            case AutoSurround.Quotes:
                serializer.Serialize(writer, "quotes");
                return;
        }
        throw new Exception("Cannot marshal type AutoSurround");
    }
}
