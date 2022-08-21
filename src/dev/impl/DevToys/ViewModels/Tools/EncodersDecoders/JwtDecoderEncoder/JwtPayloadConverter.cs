using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    internal class JwtPayloadConverter : JsonConverter<Dictionary<string, object>>
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
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();
                reader.Read();
                object? value = GetValue(ref reader);

                if (!string.IsNullOrWhiteSpace(propertyName) && value is not null)
                {
                    dictionary.Add(propertyName!, value);
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private object? GetValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long _long))
                    {
                        return _long;
                    }
                    else if (reader.TryGetDecimal(out decimal _dec))
                    {
                        return _dec;
                    }
                    throw new JsonException($"Unhandled Number value");
                case JsonTokenType.StartArray:
                    List<object?> array = new();
                    while (reader.Read() &&
                        reader.TokenType != JsonTokenType.EndArray)
                    {
                        array.Add(GetValue(ref reader));
                    }
                    return array.ToArray();
                case JsonTokenType.StartObject:
                    var result = new ExpandoObject();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        JsonTokenType tokenType = reader.TokenType;
                        string? propertyName = null;
                        if (reader.TokenType is JsonTokenType.PropertyName)
                        {
                            propertyName = reader.GetString();
                        }
                        reader.Read();
                        result.TryAdd(propertyName, GetValue(ref reader));
                    }
                    return result;
                default:
                    return JsonDocument.ParseValue(ref reader).RootElement.Clone();
            }
        }
    }
}
