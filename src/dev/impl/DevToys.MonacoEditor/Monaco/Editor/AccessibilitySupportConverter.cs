#nullable enable

using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    internal class AccessibilitySupportConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return t == typeof(AccessibilitySupport) || t == typeof(AccessibilitySupport?);
        }

        public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "auto":
                    return AccessibilitySupport.Auto;
                case "off":
                    return AccessibilitySupport.Off;
                case "on":
                    return AccessibilitySupport.On;
            }

            throw new Exception("Cannot unmarshal type AccessibilitySupport");
        }

        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (AccessibilitySupport)untypedValue;
            switch (value)
            {
                case AccessibilitySupport.Auto:
                    serializer.Serialize(writer, "auto");
                    return;
                case AccessibilitySupport.Off:
                    serializer.Serialize(writer, "off");
                    return;
                case AccessibilitySupport.On:
                    serializer.Serialize(writer, "on");
                    return;
            }

            throw new Exception("Cannot marshal type AccessibilitySupport");
        }
    }
}
