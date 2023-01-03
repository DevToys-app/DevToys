using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Control the side of the minimap in editor.
/// Defaults to 'right'.
/// </summary>
[JsonConverter(typeof(SideConverter))]
public enum Side { Left, Right };

internal class SideConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Side) || t == typeof(Side?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "left" => Side.Left,
            "right" => (object)Side.Right,
            _ => throw new Exception("Cannot unmarshal type Side"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Side)untypedValue;
        switch (value)
        {
            case Side.Left:
                serializer.Serialize(writer, "left");
                return;
            case Side.Right:
                serializer.Serialize(writer, "right");
                return;
        }
        throw new Exception("Cannot marshal type Side");
    }
}
