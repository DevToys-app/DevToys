using System.Dynamic;
using System.Text.Json;
using DevToys.Blazor.Core.Converters;

namespace DevToys.Blazor.Core.Extensions;

internal static class JsonExtensions
{
    /// <summary>
    /// Converts the given object to a dynamic object resulting from the serialization of the object using the given <paramref name="serializerOptions"/>.
    /// By default, this method will not include null values. This can be beneficial for interop scenarios where a JavaScript code handles `undefined` and `null` differently.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Blazor, passing an object to a JavaScript using <see cref="IJSRuntime.InvokeAsync{TValue}(string, CancellationToken, object?[]?)"/> will serialize the object
    /// into a JSON using a <see cref="JsonSerializerOptions"/> defined in the .NET framework.
    /// <see href="https://github.com/dotnet/aspnetcore/blob/e6be3e95fec33ca3a3b576df4b265ead680a91a0/src/JSInterop/Microsoft.JSInterop/src/JSRuntime.cs#L31-L44">See code on GitHub</see>.
    /// Unfortunately, we are not allowed to customize this JSON serialization and can have a negative impact on us occasionally. For instance, it does not allow to exclude null values.
    /// <see href="https://github.com/dotnet/aspnetcore/issues/12685">See issue #12685 on GitHub</see>
    /// </para>
    /// <para>
    /// Monaco Editor, for instance, has many properties that are optional. Most of the time, it is fine. But occasionally, Monaco Editor behavior varies based on whether
    /// a property is `null` or `undefined`.
    /// </para>
    /// <para>
    /// This <see cref="PrepareJsInterop{T}(T, JsonSerializerOptions?)"/> method is workaround to this limitation as it will (by default) exclude null values from the JSON serialization,
    /// making any .NET `null` value being interpreted as `undefined` in JavaScript.
    /// </para>
    /// <para>
    /// Due to the nature of this method, it is recommended to avoid using this method as much as possible as it can cause some significant performance issues.
    /// </para>
    /// </remarks>
    internal static object? PrepareJsInterop<T>(this T obj, JsonSerializerOptions? serializerOptions = null)
    {
        // Handle simple types
        if (obj == null)
        {
            return obj;
        }

        Type type = obj.GetType();
        if (obj.GetType().IsPrimitive || type == typeof(string))
        {
            return obj;
        }

        // Handle jsonElements
        if (obj is JsonElement jsonElement)
        {
            return jsonElement.PrepareJsonElement();
        }

        // Set default serializer options if necessary
        serializerOptions ??= new JsonSerializerOptions
        {
            MaxDepth = 32,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new PolymorphicConverterFactory()
            }
        };

        // Handle all kind of complex objects
        object? result
            = JsonSerializer
            .Deserialize<JsonElement>(
                JsonSerializer.Serialize<object>(obj, serializerOptions))
            .PrepareJsonElement();

        return result;
    }

    private static object? PrepareJsonElement(this JsonElement obj)
    {
        switch (obj.ValueKind)
        {
            case JsonValueKind.Object:
                IDictionary<string, object?> expando = new ExpandoObject();
                foreach (JsonProperty property in obj.EnumerateObject())
                {
                    expando.Add(property.Name, property.Value.PrepareJsonElement());
                }

                return expando;

            case JsonValueKind.Array:
                return obj.EnumerateArray().Select(jsonElement => jsonElement.PrepareJsonElement());

            case JsonValueKind.String:
                return obj.GetString();

            case JsonValueKind.Number:
                return obj.GetDecimal();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            case JsonValueKind.Undefined:
                return null;

            default:
                throw new NotSupportedException();
        }
    }
}
