using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

[JsonConverter(typeof(MultipleConverter))]
public enum Multiple { Goto, GotoAndPeek, Peek };

internal class MultipleConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Multiple) || t == typeof(Multiple?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "goto" => Multiple.Goto,
            "gotoAndPeek" => Multiple.GotoAndPeek,
            "peek" => (object)Multiple.Peek,
            _ => throw new Exception("Cannot unmarshal type Multiple"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Multiple)untypedValue;
        switch (value)
        {
            case Multiple.Goto:
                serializer.Serialize(writer, "goto");
                return;
            case Multiple.GotoAndPeek:
                serializer.Serialize(writer, "gotoAndPeek");
                return;
            case Multiple.Peek:
                serializer.Serialize(writer, "peek");
                return;
        }
        throw new Exception("Cannot marshal type Multiple");
    }
}
