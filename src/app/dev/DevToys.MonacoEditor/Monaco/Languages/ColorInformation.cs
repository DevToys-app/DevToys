using DevToys.MonacoEditor.JsonConverters;
using Newtonsoft.Json;
using Windows.UI;

namespace DevToys.MonacoEditor.Monaco.Languages;

/// <summary>
/// A color range is a range in a text model which represents a color.
/// </summary>
public sealed class ColorInformation
{
    [JsonProperty("color")]
    [JsonConverter(typeof(ColorConverter))]
    public Color Color { get; set; }

    [JsonProperty("range")]
    [JsonConverter(typeof(InterfaceToClassConverter<IRange, Range>))]
    public IRange Range { get; set; }

    public ColorInformation(Color color, IRange range)
    {
        Color = color;
        Range = range;
    }
}

/// <summary>
/// Helper to convert between <see cref="Windows.UI.Color"/> and Monaco <see href="https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.icolor.html">IColor</see>.
/// </summary>
internal class ColorConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Color) || t == typeof(Color?);

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        var color = new Color();

        if (reader.Read())
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.Value)
                {
                    case "alpha":
                        color.A = (byte)(reader.ReadAsDouble().GetValueOrDefault(0) * 255);
                        break;
                    case "red":
                        color.R = (byte)(reader.ReadAsDouble().GetValueOrDefault(0) * 255);
                        break;
                    case "green":
                        color.G = (byte)(reader.ReadAsDouble().GetValueOrDefault(0) * 255);
                        break;
                    case "blue":
                        color.B = (byte)(reader.ReadAsDouble().GetValueOrDefault(0) * 255);
                        break;
                }

                reader.Read(); // Advance past Number Token read above to next property
            }
        }

        return color;
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Color)untypedValue;

        writer.WriteStartObject();
        writer.WritePropertyName("alpha");
        writer.WriteValue(value.A / 255F);
        writer.WritePropertyName("red");
        writer.WriteValue(value.R / 255F);
        writer.WritePropertyName("green");
        writer.WriteValue(value.G / 255F);
        writer.WritePropertyName("blue");
        writer.WriteValue(value.B / 255F);
        writer.WriteEndObject();
    }
}
