#nullable enable

using Newtonsoft.Json;
using System;
using System.Linq;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    internal class CssStyleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(ICssStyle) || objectType.GetInterfaces().Contains(typeof(ICssStyle));

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => new NotSupportedException();

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is ICssStyle style)
            {
                writer.WriteValue(style.Name);
            }
        }
    }
}
