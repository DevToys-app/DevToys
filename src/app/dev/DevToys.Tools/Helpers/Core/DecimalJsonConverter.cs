﻿using Newtonsoft.Json;

namespace DevToys.Tools.Helpers.Core;
internal class DecimalJsonConverter : JsonConverter<decimal>
{
    public override decimal ReadJson(
        JsonReader reader,
        Type objectType,
        decimal existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
    {
        // prevent adding trailing zeros 
        writer.WriteRawValue(value.ToString());
    }
}
