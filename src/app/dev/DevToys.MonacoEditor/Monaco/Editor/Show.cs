using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Control the rendering of the minimap slider.
/// Defaults to 'mouseover'.
///
/// Controls whether the fold actions in the gutter stay always visible or hide unless the
/// mouse is over the gutter.
/// Defaults to 'mouseover'.
/// </summary>
[JsonConverter(typeof(ShowConverter))]
public enum Show { Always, Mouseover };

internal class ShowConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Show) || t == typeof(Show?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "always" => Show.Always,
            "mouseover" => (object)Show.Mouseover,
            _ => throw new Exception("Cannot unmarshal type Show"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Show)untypedValue;
        switch (value)
        {
            case Show.Always:
                serializer.Serialize(writer, "always");
                return;
            case Show.Mouseover:
                serializer.Serialize(writer, "mouseover");
                return;
        }
        throw new Exception("Cannot marshal type Show");
    }
}
