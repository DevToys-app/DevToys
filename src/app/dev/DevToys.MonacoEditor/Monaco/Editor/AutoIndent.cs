using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Enable auto indentation adjustment.
/// Defaults to false.
/// </summary>
[JsonConverter(typeof(AutoIndentConverter))]
public enum AutoIndent { Advanced, Brackets, Full, Keep, None };

internal class AutoIndentConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(AutoIndent) || t == typeof(AutoIndent?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "advanced" => AutoIndent.Advanced,
            "brackets" => AutoIndent.Brackets,
            "full" => AutoIndent.Full,
            "keep" => AutoIndent.Keep,
            "none" => (object)AutoIndent.None,
            _ => throw new Exception("Cannot unmarshal type AutoIndent"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (AutoIndent)untypedValue;
        switch (value)
        {
            case AutoIndent.Advanced:
                serializer.Serialize(writer, "advanced");
                return;
            case AutoIndent.Brackets:
                serializer.Serialize(writer, "brackets");
                return;
            case AutoIndent.Full:
                serializer.Serialize(writer, "full");
                return;
            case AutoIndent.Keep:
                serializer.Serialize(writer, "keep");
                return;
            case AutoIndent.None:
                serializer.Serialize(writer, "none");
                return;
        }
        throw new Exception("Cannot marshal type AutoIndent");
    }
}
