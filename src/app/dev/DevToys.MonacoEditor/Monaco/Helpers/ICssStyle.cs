using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Helpers;

public interface ICssStyle
{
    uint Id { get; }

    string Name { get; }

    string ToCss();
}

public static class ICssStyleExtensions
{
    public static string WrapCssClassName(this ICssStyle style, string inner)
    {
        return string.Format(".{0} {{ {1} }}", style.Name, inner);
    }
}

internal class CssStyleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(ICssStyle).IsAssignableFrom(objectType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => new NotSupportedException();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is ICssStyle style)
        {
            writer.WriteValue(style.Name);
        }
    }
}
