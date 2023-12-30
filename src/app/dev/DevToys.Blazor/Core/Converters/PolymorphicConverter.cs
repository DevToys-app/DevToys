using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevToys.Blazor.Core.Converters;

internal sealed class PolymorphicConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Guard.IsNotNull(value);
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
