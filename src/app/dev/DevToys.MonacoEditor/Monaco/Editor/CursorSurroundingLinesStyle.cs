using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Controls when `cursorSurroundingLines` should be enforced
/// Defaults to `default`, `cursorSurroundingLines` is not enforced when cursor position is
/// changed
/// by mouse.
/// </summary>
[JsonConverter(typeof(CursorSurroundingLinesStyleConverter))]
public enum CursorSurroundingLinesStyle { All, Default };

internal class CursorSurroundingLinesStyleConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(CursorSurroundingLinesStyle) || t == typeof(CursorSurroundingLinesStyle?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "all" => CursorSurroundingLinesStyle.All,
            "default" => (object)CursorSurroundingLinesStyle.Default,
            _ => throw new Exception("Cannot unmarshal type CursorSurroundingLinesStyle"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (CursorSurroundingLinesStyle)untypedValue;
        switch (value)
        {
            case CursorSurroundingLinesStyle.All:
                serializer.Serialize(writer, "all");
                return;
            case CursorSurroundingLinesStyle.Default:
                serializer.Serialize(writer, "default");
                return;
        }
        throw new Exception("Cannot marshal type CursorSurroundingLinesStyle");
    }
}
