using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Control the mouse pointer style, either 'text' or 'default' or 'copy'
/// Defaults to 'text'
/// </summary>
[JsonConverter(typeof(MouseStyleConverter))]
public enum MouseStyle { Copy, Default, Text };

internal class MouseStyleConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(MouseStyle) || t == typeof(MouseStyle?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "copy" => MouseStyle.Copy,
            "default" => MouseStyle.Default,
            "text" => (object)MouseStyle.Text,
            _ => throw new Exception("Cannot unmarshal type MouseStyle"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (MouseStyle)untypedValue;
        switch (value)
        {
            case MouseStyle.Copy:
                serializer.Serialize(writer, "copy");
                return;
            case MouseStyle.Default:
                serializer.Serialize(writer, "default");
                return;
            case MouseStyle.Text:
                serializer.Serialize(writer, "text");
                return;
        }
        throw new Exception("Cannot marshal type MouseStyle");
    }
}
