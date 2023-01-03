using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Controls if Find in Selection flag is turned on in the editor.
/// </summary>
[JsonConverter(typeof(AutoFindInSelectionConverter))]
public enum AutoFindInSelection { Always, Multiline, Never };

internal class AutoFindInSelectionConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(AutoFindInSelection) || t == typeof(AutoFindInSelection?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "always" => AutoFindInSelection.Always,
            "multiline" => AutoFindInSelection.Multiline,
            "never" => (object)AutoFindInSelection.Never,
            _ => throw new Exception("Cannot unmarshal type AutoFindInSelection"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (AutoFindInSelection)untypedValue;
        switch (value)
        {
            case AutoFindInSelection.Always:
                serializer.Serialize(writer, "always");
                return;
            case AutoFindInSelection.Multiline:
                serializer.Serialize(writer, "multiline");
                return;
            case AutoFindInSelection.Never:
                serializer.Serialize(writer, "never");
                return;
        }
        throw new Exception("Cannot marshal type AutoFindInSelection");
    }
}
