using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor;

/// <summary>
/// Selects the folding strategy. 'auto' uses the strategies contributed for the current
/// document, 'indentation' uses the indentation based folding strategy.
/// Defaults to 'auto'.
/// </summary>
[JsonConverter(typeof(FoldingStrategyConverter))]
public enum FoldingStrategy { Auto, Indentation };

internal class FoldingStrategyConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(FoldingStrategy) || t == typeof(FoldingStrategy?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        string? value = serializer.Deserialize<string>(reader);
        return value switch
        {
            "auto" => FoldingStrategy.Auto,
            "indentation" => (object)FoldingStrategy.Indentation,
            _ => throw new Exception("Cannot unmarshal type FoldingStrategy"),
        };
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (FoldingStrategy)untypedValue;
        switch (value)
        {
            case FoldingStrategy.Auto:
                serializer.Serialize(writer, "auto");
                return;
            case FoldingStrategy.Indentation:
                serializer.Serialize(writer, "indentation");
                return;
        }
        throw new Exception("Cannot marshal type FoldingStrategy");
    }
}
