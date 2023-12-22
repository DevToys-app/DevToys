using System.Collections;
using Newtonsoft.Json;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;

public class DictionaryWithSpecialEnumValueConverter<T> : JsonConverter where T : struct, Enum
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        Type valueType = objectType.GetGenericArguments()[1];
        Type intermediateDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);

        var intermediateDictionary = Activator.CreateInstance(intermediateDictionaryType) as IDictionary;
        var finalDictionary = Activator.CreateInstance(objectType) as IDictionary;

        if (intermediateDictionary is not null && finalDictionary is not null)
        {
            serializer.Populate(reader, intermediateDictionary);
            foreach (DictionaryEntry pair in intermediateDictionary)
            {
                string? key = pair.Key.ToString();
                string? value = pair.Value?.ToString();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    finalDictionary.Add(key, value!.ToEnum<T>());
            }
        }

        return finalDictionary;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }
}
