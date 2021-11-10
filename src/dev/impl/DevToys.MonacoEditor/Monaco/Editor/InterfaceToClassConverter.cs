#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Used to upcast an interface to its object type during deserialization of JSON.
    /// </summary>
    /// <typeparam name="TInterface">Type of base Interface.</typeparam>
    /// <typeparam name="TClass">Type of class to use for deserializing object with interface.</typeparam>
    internal class InterfaceToClassConverter<TInterface, TClass> : JsonConverter where TClass : TInterface, new()
    {
        public override bool CanConvert(Type objectType)
        {
            // We only want to convert objects that are of the interface.
            return objectType == typeof(TInterface);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Use the implementation type for the deserialization of the interface.
            var pop = new TClass();

            serializer.Populate(reader, pop);

            return pop;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
