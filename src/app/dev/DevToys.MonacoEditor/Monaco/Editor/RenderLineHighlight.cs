using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Enable rendering of current line highlight.
/// Defaults to all.
/// </summary>
[JsonConverter(typeof(RenderLineHighlightConverter))]
public enum RenderLineHighlight { All, Gutter, Line, None };

internal class RenderLineHighlightConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(RenderLineHighlight) || t == typeof(RenderLineHighlight?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "all" => RenderLineHighlight.All,
            "gutter" => RenderLineHighlight.Gutter,
            "line" => RenderLineHighlight.Line,
            "none" => (object)RenderLineHighlight.None,
            _ => throw new Exception("Cannot unmarshal type RenderLineHighlight"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (RenderLineHighlight)untypedValue;
        switch (value)
        {
            case RenderLineHighlight.All:
                serializer.Serialize(writer, "all");
                return;
            case RenderLineHighlight.Gutter:
                serializer.Serialize(writer, "gutter");
                return;
            case RenderLineHighlight.Line:
                serializer.Serialize(writer, "line");
                return;
            case RenderLineHighlight.None:
                serializer.Serialize(writer, "none");
                return;
        }
        throw new Exception("Cannot marshal type RenderLineHighlight");
    }
}
