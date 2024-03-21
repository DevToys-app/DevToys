using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevToys.Tools.Helpers.Core;

internal sealed class JsonWebTokenPayloadConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dictionary = new Dictionary<string, object>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString()!;
                dictionary[propertyName] = ReadPropertyValueAsObject(ref reader, true);
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
        }

        return dictionary;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    internal static object ReadPropertyValueAsObject(ref Utf8JsonReader reader, bool read = false)
    {
        if (read)
            reader.Read();

        switch (reader.TokenType)
        {
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                return ReadNumber(ref reader);
            case JsonTokenType.True:
                return true;
            case JsonTokenType.Null:
                return null!;
            case JsonTokenType.String:
                return ReadStringAsObject(ref reader);
            case JsonTokenType.StartObject:
                return ReadJsonElement(ref reader);
            case JsonTokenType.StartArray:
                return ReadJsonElement(ref reader);
            default:
                // There is something broken here as this was called when the reader is pointing at a property.
                // It must be a known Json type.
                Debug.Assert(false, $"Utf8JsonReader.TokenType is not one of the expected types: False, Number, True, Null, String, StartArray, StartObject. Is: '{reader.TokenType}'.");
                return null;
        }
    }

    internal static object ReadNumber(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt32(out int i))
            return i;
        else if (reader.TryGetInt64(out long l))
            return l;
        else if (reader.TryGetDouble(out double d))
            return d;
        else if (reader.TryGetUInt32(out uint u))
            return u;
        else if (reader.TryGetUInt64(out ulong ul))
            return ul;
        else if (reader.TryGetSingle(out float f))
            return f;
        else if (reader.TryGetDecimal(out decimal m))
            return m;

        Debug.Assert(false, "expected to read a number, but none of the Utf8JsonReader.TryGet... methods returned true.");

        return ReadJsonElement(ref reader);
    }

    internal static object ReadStringAsObject(ref Utf8JsonReader reader, bool read = false)
    {
        if (read)
        {
            reader.Read();
        }

        // returning null keeps the same logic as JsonSerialization.ReadObject
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null!;
        }

        string? originalString = reader.GetString();
        try
        {
            if (DateTime.TryParse(originalString, out DateTime dateTimeValue))
            {
                dateTimeValue = dateTimeValue.ToUniversalTime();
                string dtUniversal = dateTimeValue.ToString("o", CultureInfo.InvariantCulture);
                if (dtUniversal.Equals(originalString, StringComparison.Ordinal))
                    return dateTimeValue;
            }
        }
        catch (Exception)
        { }

        return originalString!;
    }

    internal static JsonElement ReadJsonElement(ref Utf8JsonReader reader)
    {
#if NET6_0_OR_GREATER
        JsonElement? jsonElement;
        if (JsonElement.TryParseValue(ref reader, out jsonElement))
        {
            return jsonElement.Value;
        }
        return default;
#else
            using (JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader))
                return jsonDocument.RootElement.Clone();
#endif
    }
}
