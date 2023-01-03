using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Control the wrapping of the editor.
/// When `wordWrap` = "off", the lines will never wrap.
/// When `wordWrap` = "on", the lines will wrap at the viewport width.
/// When `wordWrap` = "wordWrapColumn", the lines will wrap at `wordWrapColumn`.
/// When `wordWrap` = "bounded", the lines will wrap at min(viewport width, wordWrapColumn).
/// Defaults to "off".
/// </summary>
[JsonConverter(typeof(WordWrapConverter))]
public enum WordWrap { Bounded, Off, On, WordWrapColumn };

internal class WordWrapConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(WordWrap) || t == typeof(WordWrap?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "bounded" => WordWrap.Bounded,
            "off" => WordWrap.Off,
            "on" => WordWrap.On,
            "wordWrapColumn" => (object)WordWrap.WordWrapColumn,
            _ => throw new Exception("Cannot unmarshal type WordWrap"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (WordWrap)untypedValue;
        switch (value)
        {
            case WordWrap.Bounded:
                serializer.Serialize(writer, "bounded");
                return;
            case WordWrap.Off:
                serializer.Serialize(writer, "off");
                return;
            case WordWrap.On:
                serializer.Serialize(writer, "on");
                return;
            case WordWrap.WordWrapColumn:
                serializer.Serialize(writer, "wordWrapColumn");
                return;
        }
        throw new Exception("Cannot marshal type WordWrap");
    }
}
